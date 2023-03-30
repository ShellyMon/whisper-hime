using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using SoraBot.Basics;
using SoraBot.BLL;
using System.Text;
using YukariToolBox.LightLog;

namespace SoraBot.Series
{
    [CommandSeries]
    public class SeTuCommandSeries
    {
        [SoraCommand(CommandExpressions = new[] { "来(\\d)?[张|份|点](?:([^\\x00-\\xff]+)?)(?:\\s?)(?:([^\\x00-\\xff]+)?)([色|涩])图" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Private)]
        public async ValueTask PrivateGetSeSeImage(PrivateMessageEventArgs eventArgs)
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

        [SoraCommand(CommandExpressions = new[] { "^来([\\d|一|二|两|俩|三|四|五|六|七|八|九|十|几]*)[点|张|份](.*?)([色|涩])图$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public async ValueTask GroupGetSeTu(GroupMessageEventArgs eventArgs)
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

            Log.Info(nameof(GroupGetSeTu), $"数量: {num}");

            // 解析TAG
            var strTags = match.Groups[2].Value;
            var tags = Array.Empty<string>();

            if (!string.IsNullOrEmpty(strTags))
            {
                tags = SegmenterService.Analyze(strTags);
            }

            Log.Info(nameof(GroupGetSeTu), $"TAGS: {string.Join(',', tags)}");

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

                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-local", name);

                    if (!File.Exists(path))
                        continue;

                    // 图片太大了，发不了
                    var size = new FileInfo(path).Length;
                    if (size >= 25 * 1024 * 1024)
                        continue;

                    msgNodes.Add(new CustomNode("涩图人", eventArgs.LoginUid, $"www.pixiv.net/artworks/{item.pid}\r\n title : {item.title}\r\n 作者 : {item.author}\r\n" + SoraSegment.Image(path)));
                }

                var (status, _, _) = await eventArgs.SourceGroup.SendGroupForwardMsg(msgNodes);

                if (status.RetCode != ApiStatusType.Ok)
                {
                    await eventArgs.SourceGroup.SendGroupMessage("消息发送失败");
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

                    for (var i = 0; i < imageFetchResult.Data.Count; i++)
                    {
                        downloadTasks.Add(SetuTimeBll.DownloadImageByAriaAsync(imageFetchResult.Data[i]));
                    }

                    Task.WaitAll(downloadTasks.ToArray());

                    var msgNodes = new List<CustomNode>();

                    for (int i = 0; i < imageFetchResult.Data.Count; i++)
                    {
                        var image = imageFetchResult.Data[i];
                        var path = downloadTasks[i].Result;

                        if (string.IsNullOrEmpty(path))
                            continue;
                        if (!File.Exists(path))
                            continue;

                        // 图片太大了，发不了
                        var size = new FileInfo(path).Length;
                        if (size >= 25 * 1024 * 1024)
                            continue;

                        msgNodes.Add(new CustomNode("涩图人", eventArgs.LoginUid, $"https://www.pixiv.net/artworks/{image.PID}\r\n title : {image.Title}\r\n 作者 : {image.Author}\r\n" + SoraSegment.Image(path)));
                    }

                    if (msgNodes.Count == 0)
                    {
                        await eventArgs.SourceGroup.SendGroupMessage("没有找到图片");
                        return;
                    }

                    var (status, _, _) = await eventArgs.SourceGroup.SendGroupForwardMsg(msgNodes);

                    if (status.RetCode != ApiStatusType.Ok)
                    {
                        await eventArgs.SourceGroup.SendGroupMessage("消息发送失败");
                    }
                }
                else
                {
                    await eventArgs.SourceGroup.SendGroupMessage("没有找到图片");
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
