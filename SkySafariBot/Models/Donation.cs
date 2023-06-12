using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkySafariBot.Models
{
    public class DonationAlertsResponse
    {
        public List<Donation> Data { get; set; }
    }

    public class Donation
    {
        public string Username { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
    }
}
