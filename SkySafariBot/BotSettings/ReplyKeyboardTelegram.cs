using System;
using Telegram.Bot.Types.ReplyMarkups;

namespace SkySafariBot.BotSettings
{
    public class ReplyKeyboardTelegram
    {
        public static ReplyKeyboardMarkup CreateZodiacKeyboard()
        {
            var zodiacKeyboard = new ReplyKeyboardMarkup(
                new[]
                {
                    new []
                    {
                        new KeyboardButton("♈ Aries"),
                        new KeyboardButton("♉ Taurus"),
                        new KeyboardButton("♊ Gemini"),
                    },
                    new []
                    {
                        new KeyboardButton("♋ Cancer"),
                        new KeyboardButton("♌ Leo"),
                        new KeyboardButton("♍ Virgo"),
                    },
                    new[]
                    {
                        new KeyboardButton("♎ Libra"),
                        new KeyboardButton("♏ Scorpio"),
                        new KeyboardButton("♐ Sagittarius"),
                    },
                    new[]
                    {
                        new KeyboardButton("♑ Capricorn"),
                        new KeyboardButton("♒ Aquarius"),
                        new KeyboardButton("♓ Pisces"),
                    },
                    new[]
                    {
                        new KeyboardButton("Show your support and help our cosmic journey continue! 🌟✨ Donate now! 💖") 
                    }
                });

            zodiacKeyboard.ResizeKeyboard = true;
            zodiacKeyboard.OneTimeKeyboard = false;
            zodiacKeyboard.Selective = true;

            return zodiacKeyboard;
        }
    }
}
