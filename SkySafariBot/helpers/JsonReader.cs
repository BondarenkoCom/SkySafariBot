using Newtonsoft.Json;
using SkySafariBot.Models;
using System.Reflection;

namespace SkySafariBot.helpers
{
    public static class JsonReader
    {
        public static TelegramApiModels? GetValues()
        {
            try
            {
                string basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                string jsonFilePath = Path.Combine(basePath, "Datas", "credentials.json");

                if (!File.Exists(jsonFilePath))
                    return null;

                string json = File.ReadAllText(jsonFilePath);

                return JsonConvert.DeserializeObject<TelegramApiModels>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing element at path {ex.ToString()}");
                return null;
            }
        }
    }
}
