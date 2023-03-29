using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using SoraBot.Model;
using SqlSugar;
using SqlSugar.IOC;
using System.Data;
using System.Text.RegularExpressions;
using SoraBot.Basics;
using SoraBot.BLL;
using Sora.Entities.Segment.DataModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Threading;
using System.Text;
using System.Diagnostics;
using YukariToolBox.LightLog;

namespace SoraBot.Series
{
    [CommandSeries]
    public class setuTime
    {
        /// <summary>
        /// 私聊
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        [SoraCommand(CommandExpressions = new[] { "来(\\d)?[张|份|点](?:([^\\x00-\\xff]+)?)(?:\\s?)(?:([^\\x00-\\xff]+)?)([色|涩])图" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Private)]
        public async ValueTask setuCommand1(PrivateMessageEventArgs eventArgs)
        {
            //MatchCollection mc = ImageDownloadService.setuRegexMatches(eventArgs.Message.ToString(), eventArgs.CommandRegex[0].ToString());
            //GroupCollection groups = mc[0].Groups;
            //DataTable RandomData = SetuTimeBll.setuTimeRandom(mc);
            //if (groups[4].ToString() == "涩")
            //{
            //    for (int i = 0; i < RandomData.Rows.Count; i++)
            //    {
            //        var fileImage = System.Environment.CurrentDirectory + @"\imgs\" + System.IO.Path.GetFileName(RandomData.Rows[i]["imgUrls"].ToString());
            //        eventArgs.IsContinueEventChain = false;
            //        await eventArgs.Reply(SoraSegment.Image(fileImage));
            //    }
            //}
        }
        /// <summary>
        /// 群聊
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        [SoraCommand(CommandExpressions = new[] { "^来([\\d|一|二|两|俩|三|四|五|六|七|八|九|十|几]*)[点|张|份](.*?)([色|涩])图$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public async ValueTask setuCommand2(GroupMessageEventArgs eventArgs)
        {
            var regex = eventArgs.CommandRegex[0];
            var match = regex.Match(eventArgs.Message.ToString());

            if (match.Groups.Count != 4)
            {
                await eventArgs.SourceGroup.SendGroupMessage("命令格式错误");
                return;
            }

            // 解析数量
            var strNum = match.Groups[1].Value;
            var num = 0;

            if (string.IsNullOrEmpty(strNum))
            {
                num = 1;
            }
            else if (!int.TryParse(strNum, out num))
            {
                num = ParseChineseNumber(strNum);
            }

            if (num < 1)
            {
                await eventArgs.SourceGroup.SendGroupMessage("数量输入错误");
                return;
            }

            Log.Info(nameof(setuCommand2), $"数量: {num}");

            // 解析TAG
            var strTags = match.Groups[2].Value;
            var tags = Array.Empty<string>();

            if (!string.IsNullOrEmpty(strTags))
            {
                if (strTags.Contains(','))
                {
                    tags = strTags.Split(',');
                }
                if (strTags.Contains('，'))
                {
                    tags = strTags.Split('，');
                }
                else if (strTags.Contains(' '))
                {
                    tags = strTags.Split(' ');
                }
                else
                {
                    tags = new[] { strTags.Trim() };
                }
            }

            Log.Info(nameof(setuCommand2), $"TAGS: {string.Join(',', tags)}");

            if (tags.Length > 2)
            {
                await eventArgs.SourceGroup.SendGroupMessage("TAG最多只能两个");
                return;
            }

            // 表示图片从磁盘读取还是从网络下载
            var sourceType = match.Groups[3].Value;

            if (sourceType == "涩")
            {
                var msgNodes = new List<CustomNode>();

                var tag1 = tags.Length > 0 ? tags[0] : string.Empty;
                var tag2 = tags.Length > 1 ? tags[1] : string.Empty;

                var images = SetuTimeBll.GetRandomImageFromDatabase(num, tag1, tag2);

                for (int i = 0; i < images.Count; i++)
                {
                    var item = images[i];

                    var name = Path.GetFileName(item.imgUrls);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    var path = Path.Combine(Environment.CurrentDirectory, "imgs", name);

                    msgNodes.Add(new CustomNode("涩图人", eventArgs.Sender, $"www.pixiv.net/artworks/{item.pid}\r\n title : {item.title}\r\n 作者 : {item.author}\r\n" + SoraSegment.Image(path)));
                }

                var (status, _, _) = await eventArgs.SourceGroup.SendGroupForwardMsg(msgNodes);

                if (status.ApiMessage == "timeout")
                {
                    await eventArgs.SourceGroup.SendGroupMessage("合并转发(群)消息发送失败");
                }
            }
            else
            {
                // 拼接URL
                var urlBuilder = new StringBuilder(80);
                urlBuilder.Append($"https://api.lolicon.app/setu/v2?&r18=0&excludeAI=true&num={num}");

                // 添加TAG
                foreach (var tag in tags)
                {
                    urlBuilder.Append($"&tag={tag}");
                }

                var imageFetchResult = await ImageDownloadService.GetLoliconImage(urlBuilder.ToString());

                if (imageFetchResult?.Data?.Count > 0)
                {
                    var downloadTasks = new List<Task<string>>();

                    foreach (var e in imageFetchResult.Data)
                    {
                        downloadTasks.Add(SetuTimeBll.DownloadImageByAria(e));
                    }

                    Task.WaitAll(downloadTasks.ToArray());

                    var msgNodes = new List<CustomNode>();

                    for (int i = 0; i < imageFetchResult.Data.Count; i++)
                    {
                        var image = imageFetchResult.Data[i];

                        if (downloadTasks[i].Result != "complete")
                            continue;

                        string savePath = Path.Combine(Environment.CurrentDirectory, "img", Path.GetFileName(image.Urls.Original));

                        msgNodes.Add(new CustomNode("涩图人", eventArgs.Sender, $"www.pixiv.net/artworks/{image.PID}\r\n title : {image.Title}\r\n 作者 : {image.Author}\r\n" + SoraSegment.Image(savePath)));
                    }

                    var (status, _, _) = await eventArgs.SourceGroup.SendGroupForwardMsg(msgNodes);

                    if (status.ApiMessage == "timeout")
                    {
                        await eventArgs.SourceGroup.SendGroupMessage("合并转发(群)消息发送失败");
                    }
                }
                else
                {
                    await eventArgs.SourceGroup.SendGroupMessage("未检索成功");
                    return;
                }
            }
        }

        private static int ParseChineseNumber(string text)
        {
            return text switch
            {
                "一" or "壹" => 1,
                "二" or "貳" or "两" or "俩" => 2,
                "三" or "叁" or "几" => 3,
                "四" or "肆" => 4,
                "五" or "伍" => 5,
                "六" or "陸" => 6,
                "七" or "柒" => 7,
                "八" or "捌" => 8,
                // 其它情況返回零不作处理
                _ => 0
            };
        }
    }
}
