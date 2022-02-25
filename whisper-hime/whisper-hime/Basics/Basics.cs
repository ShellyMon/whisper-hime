using SauceNET;
using Sisters.WudiLib;
using whisper_hime.Model;
using Message = Sisters.WudiLib.SendingMessage;
namespace whisper_hime
{
    public static class Basics
    {
        //Sauce搜图接口
        public static async Task<SauceNET.Model.Sauce> ClientTest(string url)
        {
            string apiKey = "145b44762ab8a4a82dc2b1e0a2c7a40b4bdb5cc6";
            //Create your SauceNET client
            var client = new SauceNETClient(apiKey);

            //Enter your image url.
            string image = url;

            //Get the sauce
            return await client.GetSauceAsync(image);
        }
        public static Task<getImage> getImage(this HttpApiClient api, string file)
        {
            return api.CallAsync<getImage>("get_image", new
            {
                file = file
            });
        }

        [Obsolete]
        public static Dictionary<string, string> getQueryStringFromURL(string url)
        {
            var result = new Dictionary<string, string>();
            var uri = new Uri(url, true);
            var queryStrings = System.Web.HttpUtility.ParseQueryString(uri.Query);
            foreach (var key in queryStrings.Keys)
                result.Add(key.ToString(), queryStrings[key.ToString()]);
            return result;
        }

        public static object groupForwardMessage(this HttpApiClient api, long group_id, Message message)
        {
            return api.CallAsync<groupForwardMsg>("send_group_forward_msg", new
            {
                group_id = group_id,
                messages = message.Sections,
            });
        }
        public static Task<getMsg> getMsg(this HttpApiClient api, string message_id)
        {
            return api.CallAsync<getMsg>("get_msg", new
            {
                message_id = message_id,
            });
        }
    }
}
