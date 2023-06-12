using Telegram.Bot;
using Telegram.Bot.Exceptions;

namespace SkySafariBot.helpers
{
    public class HandlerError
    {
        public static async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken = default)
        {
            if (exception is ApiRequestException apiRequestException && apiRequestException.ErrorCode == 403)
            {
                Console.WriteLine("Bot was blocked by user.");
                return;
            }
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(exception));
        }
    }
}
