using SkySafariBot.helpers;
using SkySafariBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace SkySafariBot.BotSettings
{
    public class GPTDriver
    {
        public async Task<string> GenerateHoroscope(string zodiacSign, bool monthlySubs,string telegramuserName)
        {
            string apiKey = JsonReader.GetValues().openApiKey;
            string endpoint = "https://api.openai.com/v1/chat/completions";
            string contentDay = $"Generate a horoscope for {zodiacSign} for today.";
            string contentMonth = $"Generate a horoscope for {zodiacSign} for month.";
            string typeContent = string.Empty;

            List<Message> messages = new List<Message>();
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var greetingMessage = new Message() { Role = "user", Content = "You are a clairvoyant who creates horoscopes. Please provide a horoscope." };
            messages.Add(greetingMessage);

            //Change range days for future
            if (monthlySubs == false)
            {
                //var content = $"Generate a horoscope for {zodiacSign} for today.";
                typeContent = contentDay;
            }
            else if (monthlySubs == true)
            {
                typeContent = contentMonth;
            }
            else
            {
                throw new ArgumentException("monthly Subs must be either true or false");
            }


            var message = new Message() { Role = "user", Content = typeContent };
            messages.Add(message);

            var requestData = new Request()
            {
                ModelId = "gpt-3.5-turbo",
                Messages = messages
            };

            using var response = await httpClient.PostAsJsonAsync(endpoint, requestData);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"{(int)response.StatusCode} {response.StatusCode}");
                return null;
            }

            ResponseData? responseData = await response.Content.ReadFromJsonAsync<ResponseData>();

            var choices = responseData?.Choices ?? new List<Choice>();
            if (choices.Count == 0)
            {
                Console.WriteLine("No choices were returned by the API");
                return null;
            }

            var choice = choices[0];
            var responseMessage = choice.Message;
            messages.Add(responseMessage);
            var responseText = responseMessage.Content.Trim();

            string emoji = GetEmojiForZodiacSign(zodiacSign);
            string userName = telegramuserName;
            string formattedResponse = $"{emoji} {userName}, here's your horoscope for {zodiacSign} today:\n\n{responseText}\n\nHave a magical day, {userName}! {emoji}";

            Console.WriteLine($"ChatGPT: {responseText}");
            return formattedResponse;
        }

        private string GetEmojiForZodiacSign(string zodiacSign)
        {
            switch (zodiacSign.ToLower())
            {
                case "aries":
                    return "♈";
                case "taurus":
                    return "♉";
                case "gemini":
                    return "♊";
                case "cancer":
                    return "♋";
                case "leo":
                    return "♌";
                case "virgo":
                    return "♍";
                case "libra":
                    return "♎";
                case "scorpio":
                    return "♏";
                case "sagittarius":
                    return "♐";
                case "capricorn":
                    return "♑";
                case "aquarius":
                    return "♒";
                case "pisces":
                    return "♓";
                default:
                    return "";
            }
        }
    }
}
