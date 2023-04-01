﻿using Microsoft.Extensions.Logging;
using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using SoraBot.Basics;
using SoraBot.BLL;
using SoraBot.Tools;
using System.Text;

namespace SoraBot.Commands
{
    /// <summary>
    /// 色图
    /// </summary>
    [CommandSeries]
    public class SeTu
    {
        [SoraCommand(CommandExpressions = new[] { "^来([\\d|一|二|两|俩|三|四|五|六|七|八|九|十|几]*)[点|张|份](.*?)([色|涩])图$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Private)]
        public static async ValueTask PrivateGetSeTu(PrivateMessageEventArgs ev)
        {
            var logger = Ioc.Require<ILogger<SeTu>>();

            var regex = ev.CommandRegex[0];
            var match = regex.Match(ev.Message.ToString());

            if (match.Groups.Count != 4)
            {
                await ev.Reply("命令格式错误");
                return;
            }

            // 解析数量
            var strNum = match.Groups[1].Value;
            int num;

            if (string.IsNullOrEmpty(strNum))
            {
                num = 1;
            }
            else if (!int.TryParse(strNum, out num))
            {
                num = Util.ParseChineseNumber(strNum);
            }

            if (num < 1)
            {
                await ev.Reply("数量输入错误");
                return;
            }

            logger.LogInformation("数量: {}", num);

            // 解析TAG
            var strTags = match.Groups[2].Value;
            var tags = Array.Empty<string>();

            if (!string.IsNullOrEmpty(strTags))
            {
                tags = SegmenterService.Analyze(strTags);
            }

            logger.LogInformation("TAG: {}", string.Join(',', tags));

            if (tags.Length > 2)
            {
                await ev.Reply("TAG最多只能两个");
                return;
            }

            // 表示图片从磁盘读取还是从网络下载
            var sourceType = match.Groups[3].Value;

            if (sourceType == "涩")
            {
                var tag1 = tags.Length > 0 ? tags[0] : string.Empty;
                var tag2 = tags.Length > 1 ? tags[1] : string.Empty;

                var images = SeTuBll.GetRandomImageFromDatabase(num, tag1, tag2);

                for (int i = 0; i < images.Count; i++)
                {
                    var item = images[i];

                    var name = Path.GetFileName(item.Url);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-local", name);

                    if (!File.Exists(path))
                        continue;
                    if (Util.IsImageTooLarge(path))
                        continue;

                    var (status, _) = await ev.Reply($"www.pixiv.net/artworks/{item.Pid}\r\n title : {item.Title}\r\n 作者 : {item.Artist}\r\n" + SoraSegment.Image(path));

                    if (status.RetCode != ApiStatusType.Ok)
                    {
                        await ev.Reply("消息发送失败");
                    }
                }
            }
            else
            {
                // 拼接URL
                var urlBuilder = new StringBuilder(80);
                urlBuilder.Append($"https://api.lolicon.app/setu/v2?&r18=2&excludeAI=true&num={num}");

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
                        downloadTasks.Add(SeTuBll.DownloadPixivImageAsync(imageFetchResult.Data[i].Urls.Original));
                    }

                    Task.WaitAll(downloadTasks.ToArray());

                    for (int i = 0; i < imageFetchResult.Data.Count; i++)
                    {
                        var image = imageFetchResult.Data[i];
                        var path = downloadTasks[i].Result;

                        if (string.IsNullOrEmpty(path))
                            continue;
                        if (!File.Exists(path))
                            continue;
                        if (Util.IsImageTooLarge(path))
                            continue;

                        var (status, _) = await ev.Reply($"https://www.pixiv.net/artworks/{image.PID}\r\n title : {image.Title}\r\n 作者 : {image.Author}\r\n" + SoraSegment.Image(path));

                        if (status.RetCode != ApiStatusType.Ok)
                        {
                            await ev.Reply("消息发送失败");
                        }
                    }
                }
                else
                {
                    await ev.Reply("淦，老兄，你的XP好怪哦，没有找到这种图片");
                }
            }
        }

        [SoraCommand(CommandExpressions = new[] { "^来([\\d|一|二|两|俩|三|四|五|六|七|八|九|十|几]*)[点|张|份](.*?)([色|涩])图$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupGetSeTu(GroupMessageEventArgs ev)
        {
            var logger = Ioc.Require<ILogger<SeTu>>();

            var regex = ev.CommandRegex[0];
            var match = regex.Match(ev.Message.ToString());

            if (match.Groups.Count != 4)
            {
                await ev.SourceGroup.SendGroupMessage("命令格式错误");
                return;
            }

            // 解析数量
            var strNum = match.Groups[1].Value;
            int num;

            if (string.IsNullOrEmpty(strNum))
            {
                num = 1;
            }
            else if (!int.TryParse(strNum, out num))
            {
                num = Util.ParseChineseNumber(strNum);
            }

            if (num < 1)
            {
                await ev.SourceGroup.SendGroupMessage("数量输入错误");
                return;
            }

            logger.LogInformation("数量: {num}", num);

            // 解析TAG
            var strTags = match.Groups[2].Value;
            var tags = Array.Empty<string>();

            if (!string.IsNullOrEmpty(strTags))
            {
                tags = SegmenterService.Analyze(strTags);
            }

            logger.LogInformation("TAG: {}", string.Join(',', tags));

            if (tags.Length > 2)
            {
                await ev.SourceGroup.SendGroupMessage("TAG最多只能两个");
                return;
            }

            // 表示图片从磁盘读取还是从网络下载
            var sourceType = match.Groups[3].Value;

            if (sourceType == "涩")
            {
                var msgNodes = new List<CustomNode>();

                var tag1 = tags.Length > 0 ? tags[0] : string.Empty;
                var tag2 = tags.Length > 1 ? tags[1] : string.Empty;

                var images = SeTuBll.GetRandomImageFromDatabase(num, tag1, tag2);

                for (int i = 0; i < images.Count; i++)
                {
                    var item = images[i];

                    var name = Path.GetFileName(item.Url);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-local", name);

                    if (!File.Exists(path))
                        continue;
                    if (Util.IsImageTooLarge(path))
                        continue;

                    msgNodes.Add(new CustomNode("涩图人", ev.LoginUid, $"www.pixiv.net/artworks/{item.Pid}\r\n title : {item.Title}\r\n 作者 : {item.Artist}\r\n" + SoraSegment.Image(path)));
                }

                if (msgNodes.Count == 0)
                {
                    await ev.SourceGroup.SendGroupMessage("没有找到图片");
                    return;
                }

                var (status, _, _) = await ev.SourceGroup.SendGroupForwardMsg(msgNodes);

                if (status.RetCode != ApiStatusType.Ok)
                {
                    await ev.SourceGroup.SendGroupMessage("消息发送失败");
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
                        downloadTasks.Add(SeTuBll.DownloadPixivImageAsync(imageFetchResult.Data[i].Urls.Original));
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
                        if (Util.IsImageTooLarge(path))
                            continue;

                        msgNodes.Add(new CustomNode("涩图人", ev.LoginUid, $"https://www.pixiv.net/artworks/{image.PID}\r\n title : {image.Title}\r\n 作者 : {image.Author}\r\n" + SoraSegment.Image(path)));
                    }

                    if (msgNodes.Count == 0)
                    {
                        await ev.SourceGroup.SendGroupMessage("没有找到图片");
                        return;
                    }

                    var (status, _, _) = await ev.SourceGroup.SendGroupForwardMsg(msgNodes);

                    if (status.RetCode != ApiStatusType.Ok)
                    {
                        await ev.SourceGroup.SendGroupMessage("消息发送失败");
                    }
                }
                else
                {
                    await ev.SourceGroup.SendGroupMessage("淦，老兄，你的XP好怪哦，没有找到这种图片");
                }
            }
        }
    }
}