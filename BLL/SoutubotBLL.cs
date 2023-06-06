using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WhisperHime.Basics;
using WhisperHime.Dto.soutubot;

namespace WhisperHime.BLL
{
    public class SoutubotBLL
    {
        internal static async Task<(string, string)> DownloadSoutubotImageAsync(string url,string saveName)
        {
            var logger = Ioc.Require<ILogger<SeTuBll>>();


            var saveDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "soutubot-local");
             saveName += ".webp";
            var filePath = Path.Combine(saveDirPath, saveName);
            

            // 检查缓存
            if (!File.Exists(filePath))
            {
                var options = new Dictionary<string, object>()
                {
                    { "dir", saveDirPath },
                    { "out", saveName },
                };

                var status = await ImageDownloadService.DownloadFileByAriaAsync(url, options);

                if (status != "complete")
                {
                    logger.LogError("图片下载失败");
                    return (saveName, string.Empty);
                }

                await SeTuBll.ImgCompress(filePath);
            }

            if (File.Exists(filePath))
            {
                return (saveName, filePath);
            }
            else
            {
                logger.LogError("文件不存在");
                return (saveName, string.Empty);
            }
        }

        internal static async Task<Bot> RequestApiDataAsync(string saveName, string filePath)
        {
            var CHROME_UA = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36";
            var datenow = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            var api_key = WhisperHime.Tools.Util.ComputeApiKey(datenow, CHROME_UA.Length);

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Referrer = new Uri("https://soutubot.moe/");
            var content = new MultipartFormDataContent();
            client.DefaultRequestHeaders.Add("User-Agent", CHROME_UA);



            content.Headers.Add("Cookie", "_ga=GA1.1.125127865.1681439188; _ga_JB369TC9SF=GS1.1.1685950996.8.1.1685951322.0.0.0");
            content.Headers.Add("Origin", "https://soutubot.moe/");
            content.Headers.Add("x-api-key", api_key);
            content.Headers.Add("x-requested-with", "XMLHttpRequest");


            content.Add(new StringContent("1.2"), "factor");
            content.Add(new ByteArrayContent(System.IO.File.ReadAllBytes(filePath)), "file", saveName);

            var requestUri = "https://soutubot.moe/api/search";
            var result = await client.PostAsync(requestUri, content).Result.Content.ReadAsStringAsync();

            return Bot.FromJson(result);

        }
    }
}
