using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WhisperHime.Basics;
using WhisperHime.Dto.soutubot;
using Newtonsoft.Json;

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

        internal static async Task TaskAsync()
        {
            var url = "http://127.0.0.1:8191/v1";
            var jsonObj = new { cmd = "request.get", url = "https://soutubot.moe/" , maxTimeout = 60000 };
            string json = JsonConvert.SerializeObject(jsonObj);
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response = await client.PostAsync(url, new StringContent(json, System.Text.Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();

                    var responseJson = SoutuBotMods.FromJson(responseBody);
                    string cookic = "";

                    foreach (var item in responseJson.Solution.Cookies)
                    {
                        cookic += $"{item.Name}={item.Value};";
                    }
                    Cookie = cookic;
                    CHROME_UA = responseJson.Solution.UserAgent;



                    // 在这里处理响应
                }
                else
                {
                    // 处理错误
                }
            }
        }

        public static string CHROME_UA = "";
        public static string Cookie = "";

        internal static async Task<Bot> RequestApiDataAsync(string saveName, string filePath)
        {
            var datenow = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;//获取时间戳，精确到分钟
            var api_key = WhisperHime.Tools.Util.ComputeApiKey(datenow, CHROME_UA.Length);

            HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.Referrer = new Uri("https://soutubot.moe/");
            var content = new MultipartFormDataContent();

            client.DefaultRequestHeaders.Add("User-Agent", CHROME_UA);

            content.Headers.Add("Cookie", Cookie);
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
