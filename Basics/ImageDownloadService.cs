using Aria2NET;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using WhisperHime.Dto.Lolicon;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WhisperHime.Basics
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

        internal static async Task<LoliconApiResult<List<LoliconImage>>> GetLoliconImage(string url)
        {
            var json = await HttpGetAsync(url);
            var obj = JsonConvert.DeserializeObject<LoliconApiResult<List<LoliconImage>>>(json);
            return obj ?? new();
        }

        internal static async Task<List<WhisperHime.Dto.BGM.CalendarItem>> GetBgmAnimeCalendar()
        {
            var json = await HttpGetAsync("https://api.bgm.tv/calendar");
            var obj = JsonConvert.DeserializeObject<List<WhisperHime.Dto.BGM.CalendarItem>>(json);
            return obj ?? new();
        }


        internal static async Task<string> DownloadFileByAriaAsync(string url, IDictionary<string, object> options)
        {
            var logger = Ioc.Require<ILogger<ImageDownloadService>>();

            logger.LogInformation("文件 {} 开始下载", url);

            var gid = await _ariaClient.AddUriAsync(new List<string> { url }, options);

            string status;

            while (true)
            {
                var cs = await _ariaClient.TellStatusAsync(gid);
                status = cs.Status;

                if (status == "complete")
                {
                    logger.LogInformation("文件 {} 下载成功", url);
                    break;
                }
                else if (status == "error")
                {
                    // 删除失败记录
                   // await _ariaClient.RemoveDownloadResultAsync(gid);

                    logger.LogError("文件 {} 下载失败", url);
                    break;
                }

                await Task.Delay(500);
            }

            return status;
        }
    }
}
