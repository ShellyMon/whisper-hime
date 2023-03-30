using Aria2NET;
using Newtonsoft.Json;
using SoraBot.Model;

namespace SoraBot.Basics
{
    internal class ImageDownloadService
    {
        private static readonly HttpClient _httpClient;
        private static readonly Aria2NetClient _ariaClient;

        static ImageDownloadService()
        {
            _httpClient = new HttpClient();
            _ariaClient = new Aria2NetClient("http://127.0.0.1:6800/jsonrpc");
        }

        internal static async Task<string> HttpGetAsync(string url)
        {
            return await _httpClient.GetStringAsync(url);
        }

        internal static async Task<LoliconApiResult<List<LoliconImageEntity>>> GetLoliconImage(string url)
        {
            var json = await HttpGetAsync(url);
            var obj = JsonConvert.DeserializeObject<LoliconApiResult<List<LoliconImageEntity>>>(json);
            return obj ?? new();
        }

        internal static async Task<string> DownloadFileByAria(string url, IDictionary<string, object> options)
        {
            var task = await _ariaClient.AddUriAsync(new List<string> { url }, options);

            string status;

            while (true)
            {
                var cs = await _ariaClient.TellStatusAsync(task);
                status = cs.Status;

                if (status == "complete")
                {
                    break;
                }
                else if (status == "error")
                {
                    break;
                }

                await Task.Delay(500);
            }

            return status;
        }
    }
}
