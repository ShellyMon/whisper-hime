using Microsoft.Extensions.Logging;
using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using SoraBot.Basics;
using SoraBot.BLL;
using SoraBot.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    /// <summary>
    /// 色图
    /// </summary>
    [CommandSeries]
    public partial class SeTu
    {
        // 来点色图（私聊）
        [SoraCommand(CommandExpressions = new[] { SeTuRegexExpr }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Private)]
        public static async ValueTask PrivateGetSeTu(PrivateMessageEventArgs ev)
        {
            var logger = Ioc.Require<ILogger<SeTu>>();

            var (num, tags, type) = ParseSeTuCommand(ev.Message.GetText());

            logger.LogInformation("数量: {}", num);
            logger.LogInformation("标签: {}", string.Join(',', tags));

            if (tags.Length > 2)
            {
                await ev.Reply("TAG最多只能两个");
                return;
            }

            if (type == "涩")
            {
                // 从本地数据库检索

                var tag1 = tags.Length > 0 ? tags[0] : string.Empty;
                var tag2 = tags.Length > 1 ? tags[1] : string.Empty;

                var images = SeTuBll.GetRandomImageFromDatabase(num, false, tag1, tag2);

                foreach (var item in images)
                {
                    var name = Path.GetFileName(item.Url);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    var img = images[0];

                    var fileName = Path.GetFileName(img.Url);

                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-local", fileName);

                    if (!File.Exists(filePath))
                    {
                        filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache", fileName);

                        if (!File.Exists(filePath))
                        {
                            (_, filePath, _) = await SeTuBll.DownloadPixivImageAsync(img.Url);

                            if (string.IsNullOrEmpty(filePath))
                            {
                                await ev.Reply("你要找的图片失踪了");
                                return;
                            }
                        }
                    }

                    if (!File.Exists(filePath))
                        continue;
                    if (Util.IsImageTooLarge(filePath))
                        continue;

                    var msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{item.Pid}\n")
                            + SoraSegment.Text($"标题：{item.Title}\n")
                            + SoraSegment.Text($"作者：{item.Artist}\n")
                            + SoraSegment.Image(filePath);

                    var (status, _) = await ev.Reply(msg);

                    if (status.RetCode != ApiStatusType.Ok)
                    {
                        await ev.Reply($"消息发送失败");
                    }
                }
            }
            else
            {
                // 通过API检索

                var sb = new StringBuilder(128);
                sb.Append($"https://api.lolicon.app/setu/v2?&r18=2&excludeAI=true&num={num}");

                foreach (var tag in tags)
                {
                    sb.Append($"&tag={tag}");
                }

                var result = await ImageDownloadService.GetLoliconImage(sb.ToString());

                if (result?.Data?.Count > 0)
                {
                    await ReplySeTuPrivateAsync(ev, result);
                    return;
                }

                // 用关键词检索

                sb.Clear();
                sb.Append($"https://api.lolicon.app/setu/v2?&r18=2&excludeAI=true&num={num}");

                if (tags.Length == 0)
                {
                    return;
                }

                sb.Append($"&keyword={tags[0]}");

                if (tags.Length > 1)
                {
                    sb.Append($"&tag={tags[1]}");
                }

                result = await ImageDownloadService.GetLoliconImage(sb.ToString());

                if (result?.Data?.Count > 0)
                {
                    await ReplySeTuPrivateAsync(ev, result);
                }
                else
                {
                    await ev.Reply("淦，老兄，你的XP好怪哦，没有找到这种图片");
                }
            }
        }

        // 来点色图（群聊）
        [SoraCommand(CommandExpressions = new[] { SeTuRegexExpr }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupGetSeTu(GroupMessageEventArgs ev)
        {
            var logger = Ioc.Require<ILogger<SeTu>>();

            var (num, tags, type) = ParseSeTuCommand(ev.Message.GetText());

            logger.LogInformation("数量: {}", num);
            logger.LogInformation("标签: {}", string.Join(',', tags));

            if (tags.Length > 2)
            {
                await ev.Reply("TAG最多只能两个");
                return;
            }

            if (type == "涩")
            {
                // 从本地数据库检索

                var tag1 = tags.Length > 0 ? tags[0] : string.Empty;
                var tag2 = tags.Length > 1 ? tags[1] : string.Empty;

                var images = SeTuBll.GetRandomImageFromDatabase(num, false, tag1, tag2);

                var messages = new List<MessageBody>(images.Count);

                foreach (var item in images)
                {
                    var name = Path.GetFileName(item.Url);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    var img = images[0];

                    var fileName = Path.GetFileName(img.Url);

                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-local", fileName);

                    if (!File.Exists(filePath))
                    {
                        filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache", fileName);

                        if (!File.Exists(filePath))
                        {
                            (_, filePath, _) = await SeTuBll.DownloadPixivImageAsync(img.Url);

                            if (string.IsNullOrEmpty(filePath))
                            {
                                await ev.Reply("你要找的图片失踪了");
                                return;
                            }
                        }
                    }

                    if (!File.Exists(filePath))
                        continue;
                    if (Util.IsImageTooLarge(filePath))
                        continue;

                    var msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{item.Pid}\n")
                            + SoraSegment.Text($"标题：{item.Title}\n")
                            + SoraSegment.Text($"作者：{item.Artist}\n")
                            + SoraSegment.Image(filePath);

                    messages.Add(msg);
                }

                if (messages.Count == 0)
                {
                    await ev.SourceGroup.SendGroupMessage("没有找到图片");
                    return;
                }

                var forwardMsg = messages.Select(msg => new CustomNode(ev.SenderInfo.Nick, ev.SenderInfo.UserId, msg));

                var (status, _, _) = await ev.SourceGroup.SendGroupForwardMsg(forwardMsg);

                if (status.RetCode != ApiStatusType.Ok)
                {
                    await ev.SourceGroup.SendGroupMessage("消息发送失败");
                }
            }
            else
            {
                // 通过API检索

                var sb = new StringBuilder(128);
                sb.Append($"https://api.lolicon.app/setu/v2?&r18=0&excludeAI=true&num={num}");

                foreach (var tag in tags)
                {
                    sb.Append($"&tag={tag}");
                }

                var result = await ImageDownloadService.GetLoliconImage(sb.ToString());

                if (result?.Data?.Count > 0)
                {
                    await ReplySeTuGroupAsync(ev, result);
                    return;
                }

                // 用关键词检索

                sb.Clear();
                sb.Append($"https://api.lolicon.app/setu/v2?&r18=2&excludeAI=true&num={num}");

                if (tags.Length == 0)
                {
                    return;
                }

                sb.Append($"&keyword={tags[0]}");

                if (tags.Length > 1)
                {
                    sb.Append($"&tag={tags[1]}");
                }

                result = await ImageDownloadService.GetLoliconImage(sb.ToString());

                if (result?.Data?.Count > 0)
                {
                    await ReplySeTuGroupAsync(ev, result);
                }
                else
                {
                    await ev.Reply("淦，老兄，你的XP好怪哦，没有找到这种图片");
                }
            }
        }

        // 随机色图
        [SoraCommand(CommandExpressions = new[] { "^随机[色|涩]图$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupGetRandomSeTu(GroupMessageEventArgs ev)
        {
            var images = SeTuBll.GetRandomImageFromDatabase(1, false, string.Empty, string.Empty);

            if (images.Count == 0)
            {
                await ev.Reply("没有找到图片");
                return;
            }

            var img = images[0];

            // 取文件名，这两个文件夹应该要合并
            var fileName = Path.GetFileName(img.Url);

            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-local", fileName);

            if (!File.Exists(filePath))
            {
                filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache", fileName);

                if (!File.Exists(filePath))
                {
                    (_, filePath, _) = await SeTuBll.DownloadPixivImageAsync(img.Url);

                    if (string.IsNullOrEmpty(filePath))
                    {
                        await ev.Reply("你要找的图片失踪了");
                        return;
                    }
                }
            }

            var msg = SoraSegment.At(ev.Sender)
                    + SoraSegment.Text($"PID：{img.Pid}\n")
                    + SoraSegment.Text($"标题：{img.Title}\n")
                    + SoraSegment.Text($"标签：{string.Join('，', img.Tags)}\n")
                    + SoraSegment.Text($"作者：{img.Artist}\n")
                    + SoraSegment.Image(filePath);

            await ev.Reply(msg);
        }
    }
}
