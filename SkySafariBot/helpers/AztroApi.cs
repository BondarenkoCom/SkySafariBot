using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySafariBot.helpers
{

    public class AztroApi
    {
        public string today = DateTime.Today.Day.ToString();

        public async Task<string> GetToday(string sign, string day = "today")
        {
            using (var httpClient = new HttpClient())
            {
                var content = new FormUrlEncodedContent(new[]
                {
                     new KeyValuePair<string, string>("sign", sign),
                     new KeyValuePair<string, string>("day", day)
                });

                var response = await httpClient.PostAsync("https://aztro.sameerkumar.website", content);

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                else
                {
                    return "Error with POST-request";
                }
            }

        }
    }
}
