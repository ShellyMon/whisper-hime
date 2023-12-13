using Microsoft.Extensions.Logging;
using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using WhisperHime.Basics;
using WhisperHime.BLL;
using WhisperHime.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Support.UI;
using WhisperHime.Dto.Pixiv;
using WhisperHime.Entity;
using System.Linq.Expressions;

namespace WhisperHime.Commands
{
    /// <summary>
    /// 色图
    /// </summary>
    [CommandSeries]
    public partial class SeTu
    {
        // 来点色图（ALL）
        [SoraCommand(CommandExpressions = new[] { SeTuRegexExpr }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = MessageSourceMatchFlag.All)]
        public static async ValueTask PrivateGetSeTu(BaseMessageEventArgs ev)
        {
            //List<long> longs = new List<long>() 
            //{
            //    2210457659,
            //};
            //if (longs.Contains(ev.Sender.Id))
            //{
            //    if (new Random().Next(3) != 0)
            //    {
            //        return;
            //    }
            //}

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

                var messages = new List<MessageBody>();

                var xpData = SeTuBll.GetXPImageFromDatabase(tag1);
                var isXpData = false;
                if (xpData != null)
                {
                    var folderPath =  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-XP/{xpData.xpName}");
                    List<string> fileNames = Directory.GetFiles(folderPath).ToList();
                    fileNames = (List<string>)fileNames.OrderBy(x => Guid.NewGuid()).Take(new Random().Next(0,num)).ToList();

                    foreach (var item in fileNames)
                    {
                        var filePath = Path.Combine(folderPath,item);
                        await SeTuBll.ImgCompress(filePath);

                        var msg = SoraSegment.Text($"来源：{xpData.qqName}\n")
                            + SoraSegment.Image(filePath);

                        messages.Add(msg);
                    }
                    isXpData = true;
                }

                num = num - messages.Count;
                var images = SeTuBll.GetRandomImageFromDatabase(num, false, tag1, tag2);
                

                foreach (var item in images)
                {
                    var name = Path.GetFileName(item.Url);

                    if (string.IsNullOrEmpty(name))
                        continue;

                    //var img = images[0];

                    var fileName = Path.GetFileName(item.Url);

                    var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-XP/{item.Tags.Last()}/{item.Artist}", fileName);

                    if (!File.Exists(filePath))
                    {
                        var  fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache", fileName);

                        if (!File.Exists(fullPath))
                        {
                            if (isXpData)
                            {
                                fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-local/{item.Artist}", fileName);
                                if (!File.Exists(fullPath) )
                                {
                                    filePath = (await SeTuBll.DownloadPixivImageAsync(item.Url, null, $"img-XP/{xpData.xpName}/{item.Artist}")).Item2;
                                }
                                else
                                {
                                    filePath = SeTuBll.ImageMoveAsync(fullPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-XP/{xpData.xpName}/{item.Artist}"), fileName);
                                }
                            }
                            else
                            {
                                filePath = (await SeTuBll.DownloadPixivImageAsync(item.Url, null, $"img-local/{item.Artist}")).Item2;
                            }
                            if (string.IsNullOrEmpty(filePath))
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (isXpData)
                            {
                                //移动到个人目录下
                                filePath = SeTuBll.ImageMoveAsync(fullPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-XP/{xpData.xpName}/{item.Artist}"), fileName);
                            }
                            else
                            {
                                //移动到img-local目录下
                                filePath = SeTuBll.ImageMoveAsync(fullPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-local/{item.Artist}"), fileName);
                            }
                            
                        }
                    }

                    if (!File.Exists(filePath))
                        continue;
                    if (Util.IsImageTooLarge(filePath))
                        continue;

                    await SeTuBll.ImgCompress(filePath);

                    var msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{item.Pid}\n")
                            + SoraSegment.Text($"标题：{item.Title}\n")
                            + SoraSegment.Text($"作者：{item.Artist}\n")
                            + SoraSegment.Image(filePath);

                    messages.Add(msg);
                }

                if (messages.Count > 0)
                {

                    if (ev.SourceType == Sora.Enumeration.SourceFlag.Group)
                    {

                        var ee = ev as GroupMessageEventArgs;

                        var forwardMsg = messages.Select(msg => new CustomNode(ee.SenderInfo.Nick, ee.SenderInfo.UserId, msg));

                        var (status, d, c) = await ee.SourceGroup.SendGroupForwardMsg(forwardMsg);

                        if (status.RetCode != ApiStatusType.Ok)
                        {
                            await ee.Reply($"消息发送失败");
                        }
                    }

                    if (ev.SourceType == Sora.Enumeration.SourceFlag.Private)
                    {
                        var forwardMsg = messages.Select(msg => new CustomNode("涩涩人", ev.Sender.Id, msg));

                        var (status, d) = await ev.SoraApi.SendPrivateForwardMsg(ev.Sender.Id, forwardMsg);

                        if (status.RetCode != ApiStatusType.Ok)
                        {
                            await ev.Reply($"消息发送失败");
                        }
                    }

                }
                else
                {
                    await ev.Reply($"图片全都失踪了");
                }

            }
            else
            {
                // 通过API检索

                var sb = new StringBuilder(128);
                var r18 = ev.SourceType == SourceFlag.Private || ev.Sender.Id == 934678617 ? 2 : ev.SourceType == SourceFlag.Group ? 0 : 0;
                sb.Append($"https://api.lolicon.app/setu/v2?&r18={r18}&excludeAI=true&num={num}");

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
                sb.Append($"https://api.lolicon.app/setu/v2?&r18={r18}&excludeAI=true&num={num}");

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
        [SoraCommand(CommandExpressions = new[] { "^随机[色|涩]图$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = MessageSourceMatchFlag.All)]
        public static async ValueTask GroupGetRandomSeTu(BaseMessageEventArgs ev)
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
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-local/{img.Artist}", fileName);

            if (!File.Exists(filePath))
            {
                var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache", fileName);

                if (!File.Exists(fullPath))
                {
                    (_, filePath, _) = await SeTuBll.DownloadPixivImageAsync(img.Url, null, $"img-local/{img.Artist}");
                    if (string.IsNullOrEmpty(filePath))
                    {
                        await ev.Reply("没有找到图片");
                    }
                }
                else
                {
                    //移动到img-local目录下
                    filePath = SeTuBll.ImageMoveAsync(fullPath, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-local/{img.Artist}"), fileName);
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




        // 根据个人创建一个目录来存放图片（ALL）
        [SoraCommand(CommandExpressions = new[] { "^\\.XP条件 (\\w+?)$" }, MatchType = Sora.Enumeration.MatchType.KeyWord, SourceType = MessageSourceMatchFlag.All)]
        public static async ValueTask SetSetuName(BaseMessageEventArgs ev)
        {
            // 解析参数
            var match = ev.CommandRegex[0].MatchResult(ev.Message.GetText());

            // 拿获取名称
            string pathName = match[2];

            //检查是否重复添加

            //创建一个目录在本地
            var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"img-XP/{pathName}");
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }

            if (SeTuBll.setXPImageFromDatabase(ev, pathName) == 0)
            {
                await ev.Reply("创建成功");
            }
            else
            {
                await ev.Reply("创建失败或重复");
            }
        }

        // 根据个人创建一个目录来存放图片（ALL）
        [SoraCommand(CommandExpressions = new[] { ".add" }, MatchType = Sora.Enumeration.MatchType.Full, SourceType = MessageSourceMatchFlag.All, Priority = 1)]
        public static async ValueTask AddImageToDatabaseByXpImage(BaseMessageEventArgs ev)
        {

            var ImageList = ev.Message.GetAllImage();

            var xpImageData = SeTuBll.GetXPImageFromDatabase(ev.Sender.Id);
            if (xpImageData == null)
            {
                await ev.Reply("请先创建xp条件");
                return;
            }
            List<Illustration> illusts = new();
            foreach (var item in ImageList)
            {
                var prefixUrl = ev.SourceType == SourceFlag.Private ? "https://images.weserv.nl/?url=" : "";
                var netWorkUrl = prefixUrl + item.Url;
                var path = await SeTuBll.DownloadPixivImageAsync(netWorkUrl, null, $"img-XP/{xpImageData.xpName}", $"{DateTimeOffset.Now.ToUnixTimeSeconds()}.webp");

                if (string.IsNullOrEmpty(path.Item2))
                {
                    await ev.Reply("图片文件失踪了");
                    return;
                }
            }

            await ev.Reply("图片添加成功");
        }

    }

    
}
