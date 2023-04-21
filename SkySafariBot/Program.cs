using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using SkySafariBot.helpers;
using SkySafariBot.BotSettings;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Requests;

namespace TelegramBotExperiments
{

    class Program
    {
        static int lastUpdateId = 0;
        static ITelegramBotClient bot = new TelegramBotClient(JsonReader.GetValues().telegramApiToken);
        private static Dictionary<long, DateTime> LastHoroscopeDates = new Dictionary<long, DateTime>();

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                // Некоторые действия
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));

                if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                {
                    var message = update.Message;
                    string userName = string.Empty;

                    if (!string.IsNullOrEmpty(message.Chat.FirstName))
                    {
                        userName = message.Chat.FirstName;
                    }
                    else if (!string.IsNullOrEmpty(message.Chat.LastName))
                    {
                        userName = message.Chat.LastName;
                    }
                    else if (!string.IsNullOrEmpty(message.Chat.Username))
                    {
                        userName = message.Chat.Username;
                    }
                    else
                    {
                        userName = "Unknown";
                    }


                    if (message.Text.ToLower() == "/start")
                    {
                        string welcomeMessage = $"🔮✨ Welcome {userName} to our magical horoscope bot! ✨🔮\n\nWe combine ancient astrologers' wisdom with modern technology to bring you personalized horoscopes. 🌟🌙\n\nPlease choose your Zodiac sign to get started: 💫";

                        var sendingGif = await botClient.SendAnimationAsync(
                            chatId: message.Chat.Id,
                            animation: "https://media.giphy.com/media/Kbc5SZgO7re8/giphy.gif");

                        var zodiacKeyboard = ReplyKeyboardTelegram.CreateZodiacKeyboard();
                        await botClient.SendTextMessageAsync(
                            chatId: message.Chat.Id,
                            text: welcomeMessage,
                            replyMarkup: zodiacKeyboard);
                    }
                    else
                    {
                        string zodiacSign = message.Text.Trim().ToLower();
                        switch (zodiacSign)
                        {
                            case "♈ aries":
                            case "♉ taurus":
                            case "♊ gemini":
                            case "♋ cancer":
                            case "♌ leo":
                            case "♍ virgo":
                            case "♎ libra":
                            case "♏ scorpio":
                            case "♐ sagittarius":
                            case "♑ capricorn":
                            case "♒ aquarius":
                            case "♓ pisces":
                                {
                                    if (LastHoroscopeDates.TryGetValue(message.Chat.Id, out DateTime lastHoroscopeDate))
                                    {
                                        if (DateTime.UtcNow.Date == lastHoroscopeDate.Date)
                                        {
                                            var sendingGifDanger = await botClient.SendAnimationAsync(
                                            chatId: message.Chat.Id,
                                            animation: "https://media.giphy.com/media/RIFr5Mcb2Q4jS/giphy.gif");

                                            await botClient.SendTextMessageAsync(
                                                chatId: message.Chat.Id,
                                                text: $"The universe only answers you once a day, don't try to force it, dear {message.Chat.FirstName}.");
                                            return;
                                        }
                                    }


                                    var sendingGif = await botClient.SendAnimationAsync(
                                        chatId: message.Chat.Id,
                                        animation: "https://media.giphy.com/media/Y4DeltZ8VmGnTJGyPe/giphy.gif");

                                    try
                                    {
                                        // Generate the horoscope
                                        GPTDriver gptDriver = new GPTDriver();
                                        var generatedHoroscope = await gptDriver.GenerateHoroscope(zodiacSign, false, userName);

                                        await botClient.DeleteMessageAsync(
                                              chatId: sendingGif.Chat.Id,
                                              messageId: sendingGif.MessageId);

                                        // Send the horoscope
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: generatedHoroscope);

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Error generating horoscope: " + ex.ToString());
                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: "Sorry, we encountered an error generating your horoscope. Please try again later.");

                                    }

                                    LastHoroscopeDates[message.Chat.Id] = DateTime.UtcNow;
                                }
                                break;
                            case "show your support and help our cosmic journey continue! 🌟✨ donate now! 💖":
                                {
                                    var price = new List<LabeledPrice>
                                    {
                                        new LabeledPrice("Donate", 1),
                                    };

                                    var invoice = new SendInvoiceRequest(
                                        chatId: message.Chat.Id,
                                        title: "Donate",
                                        description: "Donate for us",
                                        payload: "unique_invoice_id", // unique invoice identifier
                                        providerToken: "provider_token", // your Payoneer API token
                                        currency: "USD", // currency
                                        prices: price
                                    );
                                    await botClient.SendInvoiceAsync(
                                        chatId: invoice.ChatId,
                                        title: invoice.Title,
                                        description: invoice.Description,
                                        payload: invoice.Payload,
                                        providerToken: invoice.ProviderToken,
                                        currency: invoice.Currency,
                                        prices: invoice.Prices
                                    );
                                }
                                break;
                            default:
                                await botClient.SendTextMessageAsync(message.Chat, "Sorry, I didn't understand that. Please select a Zodiac sign from the provided keyboard.");
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in HandleUpdateAsync: " + ex.ToString());
                return;
            }
        }


        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ApiRequestException apiRequestException && apiRequestException.ErrorCode == 403)
            {
                Console.WriteLine("Bot was blocked by user.");
                return;
            }
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }


        static void Main(string[] args)
        {
            Console.WriteLine("Bot is start " + bot.GetMeAsync().Result.FirstName);

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };
            bot.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cancellationToken
            );
            Console.ReadLine();
        }
    }
}