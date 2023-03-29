using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Aria2NET;
namespace SoraBot.Basics
{
    public class found
    {
        public found() { }
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
        /// <summary>
        /// 从HttpWebResponse对象中提取响应的数据转换为字符串
        /// </summary>
        /// <param name="webresponse"></param>
        /// <returns></returns>
        public static string GetResponseString(HttpWebResponse webresponse)
        {
            using (Stream s = webresponse.GetResponseStream())
            {
                StreamReader reader = new StreamReader(s, Encoding.UTF8);
                return reader.ReadToEnd();
            }
        }
        public static async Task<string> aria(string Imgurl, Dictionary<string, object> options)
        {
            var client = new Aria2NetClient("http://127.0.0.1:6800/jsonrpc");
            var result = await client.AddUriAsync(new List<String> { Imgurl }, options);
            bool Status = true;
            string msg = "";
            while (Status)
            {
                var res = await client.TellStatusAsync(result);
                if (res.Status == "complete")
                {
                    //下载成功
                    Status = false;
                }
                else if (res.Status == "active")
                {
                    //下载中
                }
                else if (res.Status == "error")
                {
                    //下载失败
                    Status = false;
                }
                msg = res.Status;
            }
            return msg;

        }
    }
}
