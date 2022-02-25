using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sisters.WudiLib;
using Message = Sisters.WudiLib.SendingMessage;
namespace whisper_hime
{
    internal class Sauce
    {
        public Sauce(Pub pub)
        {
            pub.SauceImplementEvent += new SauceImplementEventHandler(ws_SauceImplementEventAsync);//订阅发现事件
        }
        async Task ws_SauceImplementEventAsync(HttpApiClient api, Sisters.WudiLib.Posts.Message e)
        {
            bool group = false;
            if (e.MessageType is "group")
            {
                group = true;
            }
            try
            {
                string[] eee = e.RawMessage.ToString().Split(new char[2] { '[', ']' });//获取中括号内的内容
                for (int i = 0; i < eee.Length; i++)
                {
                    if (eee[i].IndexOf("CQ:image,file=") >= 0)
                    {
                        string ImageUrl = eee[i].Replace("CQ:image,file=", "");
                        var ccc = await api.getImage(ImageUrl.Split(',')[0]);
                        var clieTest = await Basics.ClientTest(ccc.url);
                        Message path = new("");
                        int num = 0;
                        for (int d = 0; d < clieTest.Results.Count; d++)
                        {
                            var clieTestD = clieTest.Results[d];
                            if (float.Parse(clieTestD.Similarity) > 62.00)//识别率
                            {
                                num++;
                                string ExtUrl = "";
                                if (clieTestD.ExtUrls != null)
                                {
                                    for (int p = 0; p < clieTestD.ExtUrls.Count; p++)
                                    {
                                        if (clieTestD.ExtUrls[p].IndexOf("chan.sankakucomplex.com") >= 0)//包含这个网站则跳出这个结果
                                        {
                                            continue;
                                        }
                                        if (clieTestD.ExtUrls[p].IndexOf(clieTestD.SourceURL) == 0)//同名则跳出结果
                                        {
                                            continue;
                                        }
                                        if (clieTestD.ExtUrls[p].IndexOf("pixiv") >= 0)
                                        {
                                            string pixivUrlPid = Basics.getQueryStringFromURL(clieTestD.ExtUrls[p])["illust_id"];
                                            ExtUrl += "https://www.pixiv.net/artworks/" + pixivUrlPid;
                                        }
                                        else
                                        {
                                            ExtUrl += clieTestD.ExtUrls[p].ToString() + "\r\n";
                                        }

                                    }
                                }
                                ExtUrl = ExtUrl == "" ? "" : "相关地址: " + ExtUrl;

                                string pixivUrl = "";
                                if (clieTestD.SourceURL != null)
                                {
                                    if (clieTestD.SourceURL.IndexOf("pixiv") >= 0)
                                    {
                                        string pixivUrlPid = Basics.getQueryStringFromURL(clieTestD.SourceURL)["illust_id"];
                                        pixivUrl += "https://www.pixiv.net/artworks/" + pixivUrlPid + "\r\n";
                                    }
                                    else
                                    {
                                        pixivUrl += clieTestD.SourceURL + "\r\n";
                                    }
                                }
                                pixivUrl = pixivUrl == "" ? "" : "传送地址: " + pixivUrl;

                                string meg = "";
                                var NetImage = Message.NetImage(clieTest.Results[d].ThumbnailURL);
                                for (int f = 0; f < clieTestD.Properties.Count; f++)
                                {
                                    if (clieTestD.Properties[f].Name == "Title")
                                    {
                                        meg += "作品标题: " + clieTestD.Properties[f].Value + "\r\n";
                                    }
                                    if (clieTestD.Properties[f].Name == "PixivId")
                                    {
                                        meg += "PixivId: " + clieTestD.Properties[f].Value + "\r\n";
                                    }
                                    if (clieTestD.Properties[f].Name == "MemberName")
                                    {
                                        meg += "作者: " + clieTestD.Properties[f].Value + "\r\n";
                                    }
                                    if (clieTestD.Properties[f].Name == "MemberId")
                                    {
                                        meg += "作者ID: " + clieTestD.Properties[f].Value + "\r\n";
                                    }

                                    if (clieTestD.Properties[f].Name == "Source")
                                    {
                                        if (clieTestD.Properties[f].Value.IndexOf("i4.pixiv.net") >= 0 || clieTestD.Properties[f].Value.IndexOf("i.pximg.net") >= 0)
                                        {
                                            //NetImage = Message.NetImage(clieTestD.Properties[f].Value);
                                            meg += "网站来源: " + clieTestD.DatabaseName + "\r\n";
                                        }
                                        else
                                        {
                                            meg += "网站来源: " + clieTestD.DatabaseName + "\r\n";
                                            meg += "相关信息: " + clieTestD.Properties[f].Value + "\r\n";
                                        }
                                    }
                                    if (clieTestD.Properties[f].Name == "Part")
                                    {
                                        meg += clieTestD.Properties[f].Value + "集\r\n";
                                    }
                                    if (clieTestD.Properties[f].Name == "EstTime")
                                    {
                                        meg += "时间: " + clieTestD.Properties[f].Value + "\r\n";
                                    }
                                }
                                string pathIma = @"[CQ:image,file=" + clieTest.Results[d].ThumbnailURL + "]";
                                Section sections = new Section("node",
                                    ("name", "涩图人"), ("uin", e.UserId.ToString()),
                                    ("content", pixivUrl + meg + ExtUrl + pathIma));
                                path += new Message(sections);
                                //var GroupResponse = api.SendMessageAsync(e.Endpoint,pixivUrl + meg + ExtUrl + NetImage);
                            }
                        }
                        if (group)
                        {
                            if (num == 0)
                            {
                                var GroupResponse = api.SendMessageAsync(e.Endpoint, Message.At(e.UserId) + "没能搜索到相关数据");
                            }
                            else
                            {
                                var group_id = long.Parse(JObject.Parse(JsonConvert.SerializeObject(e.Endpoint, Formatting.Indented))["group_id"].ToString());
                                var GroupResponse = api.groupForwardMessage(group_id, path);
                            }

                        }
                    }
                }
            }
            catch
            {

            }

        }
    }
}
