using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Polling;
using SkySafariBot.helpers;
using SkySafariBot.BotSettings;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotExperiments
{
    class Program
    {
        static int lastUpdateId = 0;

        //static ITelegramBotClient bot = new TelegramBotClient(JsonReader.GetValues().telegramApiTokenDraft);
        private static readonly ITelegramBotClient bot = new TelegramBotClient(SkySafariBot.helpers.JsonReader.GetValues().telegramApiToken);
        private static Dictionary<long, DateTime> LastHoroscopeDates = new Dictionary<long, DateTime>();
        private static Dictionary<long, DateTime> activeUsers = new Dictionary<long, DateTime>();
        private static long yourUserId = 378537742;

        public static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            try
            {
                int activeUsersCount = GetActiveUsersCount(TimeSpan.FromHours(24));
                long myChatId = 5686406180;

                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(update));
                string updateJson = Newtonsoft.Json.JsonConvert.SerializeObject(update);

                Console.WriteLine($"Active users {GetActiveUsersCount(TimeSpan.FromHours(24))}");

                try
                {
                    await botClient.SendTextMessageAsync(
                             chatId: myChatId,
                             text: $"Active users:{activeUsersCount} \n User action: {updateJson}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Problem with your chatId - {myChatId} \n error message - {ex.Message}");
                }

                try
                {
                    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.Message)
                    {
                        var message = update.Message;
                        string chatUserName = string.Empty;


                        if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Group || message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Supergroup)
                        {
                            Console.WriteLine($"I see chat number - {message.Chat.Id} chat name");
                            if (message.Text.ToLower().StartsWith("/gethoroscope"))
                            {
                                string command;
                                chatUserName = message.Chat.FirstName;

                                if (message.Text.ToLower().Contains("@"))
                                {
                                    string[] parts = message.Text.ToLower().Split('@');
                                    command = parts[0].Trim();
                                }
                                else
                                {
                                    command = message.Text.ToLower().Trim();
                                }

                                if (command == "/gethoroscope")
                                {
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        text: "click reply and spell your sign");
                                }
                            }
                            else if (message.ReplyToMessage != null && message.ReplyToMessage.From.Id == bot.BotId)
                            {
                                if (message.ReplyToMessage.Text == "click reply and spell your sign")
                                {
                                    string userSign = message.Text.Trim().ToLower();

                                    var sendingGif = await botClient.SendAnimationAsync(
                                        chatId: message.Chat.Id,
                                        animation: "https://media.giphy.com/media/Y4DeltZ8VmGnTJGyPe/giphy.gif");

                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        text: $"You replied with sign: {userSign}");

                                    try
                                    {
                                        GPTDriver gptDriver = new GPTDriver();
                                        var generatedHoroscope = await gptDriver.GenerateHoroscope(userSign, false, chatUserName);

                                        await botClient.DeleteMessageAsync(
                                              chatId: sendingGif.Chat.Id,
                                              messageId: sendingGif.MessageId);

                                        await botClient.SendTextMessageAsync(
                                            chatId: message.Chat.Id,
                                            text: generatedHoroscope);

                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error in gpt from chats -{ex.Message}");
                                    }
                                }
                            }
                        }
                        else if (message.Text.ToLower().StartsWith("/postnow"))
                        {
                            // Check if the message is from you
                            if (message.From.Id == yourUserId)
                            {
                                await PostDailyForecast();
                            }
                        }
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine($"Problem in handler with chat - {ex}");
                }

                try
                {
                    if (update.Type == Telegram.Bot.Types.Enums.UpdateType.ChannelPost)
                    {
                        var channelPost = update.ChannelPost;
                        await PostDailyForecast();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Problem in channel - {ex}");
                }

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
                        long userId = update.Message.Chat.Id;
                        activeUsers[userId] = DateTime.UtcNow;

                        string welcomeMessage =
                            $"🔮✨ Welcome {userName} to our magical horoscope bot! ✨🔮\n\nWe combine ancient astrologers' wisdom with modern technology to bring you personalized horoscopes. 🌟🌙\n\nPlease choose your Zodiac sign to get started: 💫\n\nTo use the bot in a group chat, enter the command /GetHoroscope followed by your Zodiac sign. For example: '/GetHoroscope Aries'";

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
                                        GPTDriver gptDriver = new GPTDriver();
                                        var generatedHoroscope = await gptDriver.GenerateHoroscope(zodiacSign, false, userName);

                                        await botClient.DeleteMessageAsync(
                                              chatId: sendingGif.Chat.Id,
                                              messageId: sendingGif.MessageId);

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
                                    long userId = update.Message.Chat.Id;
                                    activeUsers[userId] = DateTime.UtcNow;
                                }
                                break;
                            case "show your support and help our cosmic journey continue! 🌟✨ donate now! 💖":
                                {
                                    var inlineKeyboard = new InlineKeyboardMarkup(new[]
                                    {
                                        new []
                                        {
                                            InlineKeyboardButton.WithUrl("Donate", "https://www.donationalerts.com/r/cyborgnull"),
                                        }
                                    });
                                    await botClient.SendTextMessageAsync(
                                        chatId: message.Chat.Id,
                                        text: "Thank you for your support! Click the button below to make a donation.",
                                        replyMarkup: inlineKeyboard
                                    );
                                    break;
                                }
                            default:
                                if (message.Chat.Type == Telegram.Bot.Types.Enums.ChatType.Private) // Check if the chat type is private
                                {
                                    await botClient.SendTextMessageAsync(message.Chat,
                                        "Sorry, I didn't understand that. Please select a Zodiac sign from the provided keyboard.");
                                }
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

        private static Task HandleGroupMessageAsync(ITelegramBotClient botClient, SkySafariBot.Models.Message message)
        {
            throw new NotImplementedException();
        }

        public static int GetActiveUsersCount(TimeSpan timeSpan)
        {
            DateTime now = DateTime.UtcNow;
            return activeUsers.Count(user => (now - user.Value) <= timeSpan);
        }
        //@botTestPlatform
        //@astro_inspector

        private static readonly Dictionary<string, string> ZodiacSignsToGifs = new Dictionary<string, string>
        {
            { "aries", "https://media.giphy.com/media/Gw5zWpJYmGbf0UzxQz/giphy.gif" },
            { "taurus", "https://media.giphy.com/media/LkORKFwcgsZbNjs0eO/giphy.gif" },
            { "gemini", "https://media.giphy.com/media/QaGz7WYoPQrCNHUbBm/giphy.gif" },
            { "cancer", "https://link_to_cancer_gif" },
            { "leo", "https://media.giphy.com/media/JrYT61vIORMiiZlEdj/giphy.gif" },
            { "virgo", "https://media.giphy.com/media/d7HP2uLhJRtp5st0kB/giphy.gif" },
            { "libra", "https://media.giphy.com/media/MXFS0A3YKgGf6oUtBr/giphy.gif" },
            { "scorpio", "https://media.giphy.com/media/WUZzmRX6gD2fLZ3gQx/giphy.gif" },
            { "sagittarius", "https://media.giphy.com/media/PkpayAo7l44ghrAFMr/giphy.gif" },
            { "capricorn", "https://media.giphy.com/media/Q8Ct26pYwrMriLgEUa/giphy.gif" },
            { "aquarius", "https://media.giphy.com/media/SqU2QEzXM8kJO7TpNm/giphy.gif" },
            { "pisces", "https://media.giphy.com/media/H1fA2pKX7fNVWa77sA/giphy.gif" },
        };


        private static async Task PostDailyForecast()
        {
            Console.WriteLine("Post process is starting");

            foreach (var zodiacSign in ZodiacSignsToGifs.Keys)
            {
                try
                {
                    GPTDriver gptDriver = new GPTDriver();
                    var generatedHoroscope = await gptDriver.GenerateHoroscope(zodiacSign, false, zodiacSign);

                    await bot.SendAnimationAsync(
                            chatId: "@astro_inspector",
                            animation: ZodiacSignsToGifs[zodiacSign],
                            caption: generatedHoroscope
                        );
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error while posting forecast for " + zodiacSign);
                    Console.WriteLine("Exception Message: " + ex.Message);
                    if (ex is Telegram.Bot.Exceptions.ApiRequestException apiEx)
                    {
                        Console.WriteLine("Error Code: " + apiEx.ErrorCode);
                        //Console.WriteLine("API Response Description: " + apiEx.ApiResponseDescription);
                    }
                    Console.WriteLine("Exception StackTrace: " + ex.StackTrace);
                }
            }
        }

        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Bot is start " + bot.GetMeAsync().Result.FirstName);
                //var timer = new System.Timers.Timer(TimeSpan.FromHours(24).TotalMilliseconds);
                //var timer = new System.Timers.Timer(TimeSpan.FromMinutes(10).TotalMilliseconds);
                //timer.Elapsed += async (sender, e) => await PostDailyForecast();
                //timer.Start();
                //Console.WriteLine($"timer is starting");
                var cts = new CancellationTokenSource();
                var cancellationToken = cts.Token;
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = { }, // receive all update types
                };
                bot.StartReceiving(
                    HandleUpdateAsync,
                    HandlerError.HandleErrorAsync, // Pass the method directly here
                    receiverOptions,
                    cancellationToken
                );
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error in program.cs - {ex.Message}");
            }
        }
    }
}