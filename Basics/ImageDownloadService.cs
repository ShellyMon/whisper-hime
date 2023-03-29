using Aria2NET;
using Newtonsoft.Json;
using SoraBot.Model;

namespace SoraBot.Basics
{
    public class ImageDownloadService
    {
        internal static async Task<string> HttpGetAsync(string url)
        {
            var client = new HttpClient();
            return await client.GetStringAsync(url);
        }

        internal static async Task<LoliconApiResult<List<LoliconImageEntity>>> GetLoliconImage(string url)
        {
            var json = await HttpGetAsync(url);
            return JsonConvert.DeserializeObject<LoliconApiResult<List<LoliconImageEntity>>>(json);
        }

        internal static async Task<string> DownloadFileByAria(string url, IDictionary<string, object> options)
        {
            var aria = new Aria2NetClient("http://127.0.0.1:6800/jsonrpc");

            var task = await aria.AddUriAsync(new List<string> { url }, options);

            string status;

            while (true)
            {
                var cs = await aria.TellStatusAsync(task);
                status = cs.Status;

                if (status == "complete")
                {
                    break;
                }
                else if (status == "error")
                {
                    break;
                }
            }

            return status;
        }
    }
}
