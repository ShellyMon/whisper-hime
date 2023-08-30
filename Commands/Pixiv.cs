using LiteDB;
using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using WhisperHime.Basics;
using WhisperHime.BLL;
using WhisperHime.Dto.Pixiv;
using WhisperHime.Entity;
using WhisperHime.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WhisperHime.Commands
{
    /// <summary>
    /// Pixiv图片命令
    /// </summary>
    [CommandSeries]
    public partial class Pixiv
    {

        [SoraCommand(CommandExpressions = new[] { "^\\.pid (\\d+?)$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = MessageSourceMatchFlag.All, SuperUserCommand = false)]
        public static async ValueTask GetImageByPid(BaseMessageEventArgs ev)
        {
            // 解析参数
            var match = ev.CommandRegex[0].MatchResult(ev.Message.GetText());

            // 拿PID
            var pid = long.Parse(match[1]);
            // 获取图片详情
            var image = await PixivBll.GetImageByPidAsync(pid);

            if (image == null)
            {
                await ev.Reply("找不到图片");
                return;
            }

            if (image.MetaPages.Length > 0)
            {
                // 多张图片
                var tasks = new List<Task<(string, string, object?)>>(image.MetaPages.Length);
                var count = 0;

                foreach (var page in image.MetaPages)
                {
                    if (count++ == 10)
                        break;

                    var url = Util.ChoiceGoodQualityImageUrl(page.ImageUrls);

                    if (string.IsNullOrEmpty(url))
                        continue;

                    tasks.Add(SeTuBll.DownloadPixivImageAsync(url));
                }

                var paths = await Task.WhenAll(tasks);

                var msg = SoraSegment.Text($"标题：{image.Title}\n")
                        + SoraSegment.Text($"标签：{Util.MakeTagString(image.Tags)}\n")
                        + SoraSegment.Text($"作者：{image.User.Name}\n");

                foreach (var path in paths)
                {
                    if (string.IsNullOrEmpty(path.Item2))
                        continue;
                    msg += SoraSegment.Image(path.Item2);
                }

                await ev.Reply(msg);
            }
            else
            {
                // 单张图片
                var url = image.MetaSinglePage.OriginalImageUrl;

                if (string.IsNullOrEmpty(url))
                {
                    await ev.Reply("图片链接失踪了");
                    return;
                }

                var path = await SeTuBll.DownloadPixivImageAsync(url);

                if (string.IsNullOrEmpty(path.Item2))
                {
                    await ev.Reply("图片文件失踪了");
                    return;
                }

                var msg = SoraSegment.Text($"标题：{image.Title}\n")
                        + SoraSegment.Text($"标签：{Util.MakeTagString(image.Tags)}\n")
                        + SoraSegment.Text($"作者：{image.User.Name}\n")
                        + SoraSegment.Image(path.Item2);

                await ev.Reply(msg);
            }
        }

        [SoraCommand(CommandExpressions = new[] { "^\\.add (\\d+?)$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = MessageSourceMatchFlag.All, SuperUserCommand = false)]
        public static async ValueTask AddImageToDatabaseByPidG(BaseMessageEventArgs ev)
        {
            // 解析参数
            var match = ev.CommandRegex[0].MatchResult(ev.Message.GetText());

            // 拿PID
            var pid = long.Parse(match[1]);
            // 获取图片详情
            var image = await PixivBll.GetImageByPidAsync(pid);

            if (image == null)
            {
                await ev.Reply("找不到图片");
                return;
            }

            await ev.Reply("图片信息获取成功，开始下载图片...");

            // 该作品的所有图片
            var illusts = new List<Illustration>();

            if (image.MetaPages.Length > 0)
            {
                // 多张图片
                var tasks = new List<Task<(string, string, object?)>>(image.MetaPages.Length);
                var count = 0;

                foreach (var page in image.MetaPages)
                {
                    if (count++ == 10)
                        break;

                    var url = Util.ChoiceGoodQualityImageUrl(page.ImageUrls);

                    if (string.IsNullOrEmpty(url))
                        continue;

                    tasks.Add(SeTuBll.DownloadPixivImageAsync(url));
                }

                var paths = await Task.WhenAll(tasks);

                foreach (var path in paths)
                {
                    if (string.IsNullOrEmpty(path.Item2))
                        continue;

                    illusts.Add(new Illustration
                    {
                        Pid = image.Id,
                        p = image.p,
                        Title = image.Title,
                        Description = image.Caption,
                        Tags = image.Tags.Select(x => x.Name).ToList(),
                        IsAdult = image.XRestrict != 0,
                        Url = path.Item1,
                        Uid = image.User.Id,
                        Artist = image.User.Name,
                    });
                }
            }
            else
            {
                // 单张图片
                var url = image.MetaSinglePage.OriginalImageUrl;

                if (string.IsNullOrEmpty(url))
                {
                    await ev.Reply("图片链接失踪了");
                    return;
                }

                var path = await SeTuBll.DownloadPixivImageAsync(url);

                if (string.IsNullOrEmpty(path.Item2))
                {
                    await ev.Reply("图片文件失踪了");
                    return;
                }

                illusts.Add(new Illustration
                {
                    Pid = image.Id,
                    p = image.p,
                    Title = image.Title,
                    Description = image.Caption,
                    Tags = image.Tags.Select(x => x.Name).ToList(),
                    IsAdult = image.XRestrict != 0,
                    Url = url,
                    Uid = image.User.Id,
                    Artist = image.User.Name,
                });
            }

            if (illusts.Count > 0)
            {
                var db = Ioc.Require<ILiteDatabase>();
                var col = db.GetCollection<Illustration>();
                var total = 0;

                // 查重新增
                foreach (var illust in illusts)
                {
                    var name = Path.GetFileName(illust.Url);

                    if (!col.Exists(x => x.Url.Contains(name)))
                    {
                        col.Insert(illust);
                        total++;
                    }
                }

                await ev.Reply($"添加了{total}张图片到数据库");
            }
            else
            {
                await ev.Reply("没有图片被添加到数据库");
            }
        }

        [SoraCommand(CommandExpressions = new[] { ".排行榜" }, MatchType = Sora.Enumeration.MatchType.KeyWord, SourceType = MessageSourceMatchFlag.All, SuperUserCommand = false)]
        public static async ValueTask GetImageByRanking(BaseMessageEventArgs ev)
        {
            string RankMoth = ev.Message.RawText.Split('.')[0];

            if (string.IsNullOrEmpty(RankMoth))
                RankMoth = "month";
            else
                RankMoth = Util.ParseChineseRanking(RankMoth);

            // 获取图片详情
            var image = await PixivBll.GetImageByRankingAsync(RankMoth);

            if (image == null)
            {
                await ev.Reply("找不到图片");
                return;
            }

            var messages = new List<MessageBody>();

            var taskImagePage = new List<Task<MessageBody>>();
            foreach (var imagePage in image)
            {
                taskImagePage.Add(DownPixivRank(imagePage, ev));
            }

            var paths = await Task.WhenAll(taskImagePage);

            foreach (var msg in taskImagePage)
            {
                if (msg.Result != null)
                    messages.Add(msg.Result);
            }

            if (messages.Count == 0)
            {
                await ev.Reply("没有找到图片");
                return;
            }

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

            //var forwardMsg = messages.Select(msg => new CustomNode(ev.SenderInfo.Nick, ev.SenderInfo.UserId, msg));

            //var (status, _, _) = await ev.SourceGroup.SendGroupForwardMsg(forwardMsg);

            //if (status.RetCode != ApiStatusType.Ok)
            //{
            //    await ev.SourceGroup.SendGroupMessage("消息发送失败");
            //}

        }

        
    }
}
