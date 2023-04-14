using SauceNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperHime.Basics
{
    internal class SauceClientService
    {
        public static async Task<SauceNET.Model.Sauce> SauceResult(string url)
        {
            string apiKey = "6f55995dbcb10d878562f34408d02fa003c95215";
            var client = new SauceNETClient(apiKey);

            var sauce = await client.GetSauceAsync(url);

            return sauce;

        }
    }
}