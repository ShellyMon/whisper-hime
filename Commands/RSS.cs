using CodeHollow.FeedReader;
using Sora.Attributes.Command;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Threading.Tasks;
namespace SoraBot.Commands
{
    [CommandSeries]
    public class RSS
    {
        [SoraCommand(CommandExpressions = new[] { "游戏打折" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Private, SuperUserCommand = false)]
        public static async ValueTask GetGameListDiscount(PrivateMessageEventArgs ev)
        {
            


            var proxy = new WebProxy("socks5://127.0.0.1:10808");
            var clientHandler = new HttpClientHandler { Proxy = proxy };
            HttpClient _httpClient = new HttpClient(clientHandler);
            var request = new HttpRequestMessage(HttpMethod.Get, "https://rsshub.app/yxdzqb/popular_cn");
            var response = await _httpClient.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            await File.WriteAllTextAsync("rss/xx.xml", json);

            var feed = await FeedReader.ReadFromFileAsync("rss/xx.xml");
            Console.WriteLine("Feed Title: " + feed.Title);
            Console.WriteLine("Feed Description: " + feed.Description);
            Console.WriteLine("Feed Image: " + feed.ImageUrl);
            // ...
            foreach (var item in feed.Items)
            {
                Console.WriteLine(item.Title + " - " + item.Link);
            }
        }
    }
}
