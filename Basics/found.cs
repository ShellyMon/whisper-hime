using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Aria2NET;
using Newtonsoft.Json;
using SoraBot.Model;

namespace SoraBot.Basics
{
    public class ImageDownloadService
    {
        public ImageDownloadService() { }

        public static MatchCollection setuRegexMatches(string str,string Regex) {
            Regex regex = new Regex(Regex);
            return regex.Matches(str);
        }
        /// <summary>
        /// 发送http Get请求
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static HttpWebResponse CreateGetHttpResponse(string url)
        {
            Thread.Sleep(20);
            var request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";//链接类型
            //"Accept-Language": "zh-CN,zh;"
            request.Accept = "zh-CN,zh;";
            return request.GetResponse() as HttpWebResponse;
        }
        public static async Task<string> HttpGetAsync(string url)
        {
            var client = new HttpClient();
            return await client.GetStringAsync(url);
        }
        internal static async Task<LoliconApiResult<List<LoliconImageEntity>>> GetLoliconImage(string url)
        {
            var json = await HttpGetAsync(url);
            return JsonConvert.DeserializeObject<LoliconApiResult<List<LoliconImageEntity>>>(json);
        }

        public static string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }

        public static async Task<string> DownloadFileByAria(string url, IDictionary<string, object> options)
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
