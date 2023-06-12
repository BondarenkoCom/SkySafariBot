using Newtonsoft.Json;
using SkySafariBot.Models;
using System.Net.Http.Headers;

namespace SkySafariBot.helpers
{
    public class RequestDonate
    {
        public async Task<List<string>> SendDonate()
        {
            string donationAlertsToken = "V2PJKZTaxriZYfv3Zx21Q4arqmPUrIS95tkmaCr0";
            string donationAlertsUrl = "https://www.donationalerts.com/api/v1/alerts/donations";

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", donationAlertsToken);
            HttpResponseMessage response = await client.GetAsync(donationAlertsUrl);
            List<string> donationMessages = new List<string>();

            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync();

                var donations = JsonConvert.DeserializeObject<DonationAlertsResponse>(content);

                foreach (var donation in donations.Data)
                {
                    string messageDon = $"Donation received from {donation.Username}: {donation.Amount} {donation.Currency}";
                    donationMessages.Add(messageDon);
                }
            }

            return donationMessages;
        }
    }
}
