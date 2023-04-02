using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using SoraBot.Basics;
using SoraBot.BLL;
using SoraBot.Dto.Lolicon;
using SoraBot.Dto.Pixiv;
using SoraBot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    public partial class SeTu
    {
        private const string SeTuRegexExpr = "^来([\\d|一|二|两|俩|三|四|五|六|七|八|九|十|几]*)[点|张|份](.*?)([色|涩])图$";

        private static (int, string[], string) ParseSeTuCommand(string input)
        {
            var result = new Regex(SeTuRegexExpr).MatchResult(input);

            // 数量
            var strNum = result[1];
            int num;

            if (string.IsNullOrEmpty(strNum))
            {
                num = 1;
            }
            else if (!int.TryParse(strNum, out num))
            {
                num = Util.ParseChineseNumber(strNum);
            }

            // 标签
            var strTags = result[2];
            var tags = Array.Empty<string>();

            if (!string.IsNullOrEmpty(strTags))
            {
                tags = SegmenterService.Analyze(strTags);
            }

            // 来源
            var type = result[3];

            return (num, tags, type);
        }

        private static async Task ReplySeTuPrivateAsync(PrivateMessageEventArgs ev, LoliconApiResult<List<LoliconImage>> images)
        {
            var tasks = new LinkedList<Task<(string, string, object?)>>();
            var failedTasks = new HashSet<LoliconImage>();

            // 开始下载所有图片
            foreach (var image in images.Data)
            {
                tasks.AddLast(SeTuBll.DownloadPixivImageAsync(image.Urls.Original, image));
            }

            // 有任何下载任务完成都尽快处理
            while (tasks.Count > 0)
            {
                var task = await Task.WhenAny(tasks);
                tasks.Remove(task);

                var result = task.Result;

                var image = (result.Item3 as LoliconImage)!;
                var path = result.Item2;

                if (string.IsNullOrEmpty(path))
                {
                    // 图片下载失败，就直接从Pixiv找图，尝试获取新链接
                    if (!failedTasks.Contains(image))
                    {
                        // 防止一直下载失败造成递归
                        failedTasks.Add(image);

                        var detail = await PixivBll.GetImageByPidAsync(image.PID);

                        if (detail == null)
                            continue;

                        if (detail.MetaPages.Length > 1)
                        {
                            foreach (var page in detail.MetaPages)
                            {
                                var url = Util.ChoiceGoodQualityImageUrl(page.ImageUrls);

                                if (string.IsNullOrEmpty(url))
                                    continue;

                                tasks.AddLast(SeTuBll.DownloadPixivImageAsync(url, image));
                            }
                        }
                        else
                        {
                            var url = detail?.MetaSinglePage?.OriginalImageUrl;

                            if (!string.IsNullOrEmpty(url))
                            {
                                tasks.AddLast(SeTuBll.DownloadPixivImageAsync(url, image));
                            }
                        }
                    }
                }
                else
                {
                    if (Util.IsImageTooLarge(path))
                        continue;

                    var msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{image.PID}\n")
                            + SoraSegment.Text($"标题：{image.Title}\n")
                            + SoraSegment.Text($"作者：{image.Author}\n")
                            + SoraSegment.Image(path);

                    var (status, _) = await ev.Reply(msg);

                    if (status.RetCode != ApiStatusType.Ok)
                    {
                        await ev.Reply($"消息发送失败");
                    }
                }
            }
        }

        private static async Task ReplySeTuGroupAsync(GroupMessageEventArgs ev, LoliconApiResult<List<LoliconImage>> images)
        {
            var tasks = new List<Task<(string, string, object?)>>(images.Data.Count);

            // 开始下载所有图片
            foreach (var image in images.Data)
            {
                tasks.Add(SeTuBll.DownloadPixivImageAsync(image.Urls.Original, image));
            }

            // 等待图片下载完成
            await Task.WhenAll(tasks);

            // 下载成功的图片
            var downloadedImages = new List<Tuple<object, string>>(images.Data.Count);
            // 图片信息下载列表
            var detailTasks = new List<Task<Illust?>>(images.Data.Count);

            // 把所有下载成功的图片添加到完成列表，下载失败的尝试直接从Pixiv获取信息
            foreach (var task in tasks)
            {
                var image = (task.Result.Item3 as LoliconImage)!;
                var path = task.Result.Item2;

                if (string.IsNullOrEmpty(path))
                {
                    // 添加到详情下载列表
                    detailTasks.Add(PixivBll.GetImageByPidAsync(image.PID));
                }
                else
                {
                    if (Util.IsImageTooLarge(path))
                        continue;

                    // 添加到完成列表
                    downloadedImages.Add(Tuple.Create((object)image, path));
                }
            }

            // 等待图片信息下载完成
            await Task.WhenAll(detailTasks);

            // 这里就直接复用上面的变量了
            tasks.Clear();

            // 从Pixiv获取信息后，再次尝试下载这些图片
            foreach (var task in detailTasks)
            {
                var detail = task.Result;

                if (detail == null)
                    continue;

                // 暂不支持多图作品
                if (detail.MetaPages.Length > 1)
                    continue;

                var url = detail?.MetaSinglePage?.OriginalImageUrl;

                if (string.IsNullOrEmpty(url))
                    continue;

                tasks.Add(SeTuBll.DownloadPixivImageAsync(url, detail));
            }

            // 等待图片下载完成
            await Task.WhenAll(tasks);

            // 把所有下载成功的图片添加到列表
            foreach (var task in tasks)
            {
                var image = (task.Result.Item3 as Illust)!;
                var path = task.Result.Item2;

                if (string.IsNullOrEmpty(path))
                    continue;
                if (Util.IsImageTooLarge(path))
                    continue;

                downloadedImages.Add(Tuple.Create((object)image, path));
            }

            // 构建消息
            var messages = new List<MessageBody>(downloadedImages.Count);

            foreach (var image in downloadedImages)
            {
                MessageBody? msg = null;

                if (image.Item1 is LoliconImage a)
                {
                    msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{a.PID}\n")
                        + SoraSegment.Text($"标题：{a.Title}\n")
                        + SoraSegment.Text($"作者：{a.Author}\n")
                        + SoraSegment.Image(image.Item2);
                }
                else if (image.Item1 is Illust b)
                {
                    msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{b.Id}\n")
                        + SoraSegment.Text($"标题：{b.Title}\n")
                        + SoraSegment.Text($"作者：{b.User.Name}\n")
                        + SoraSegment.Image(image.Item2);
                }

                if (msg != null)
                {
                    messages.Add(msg);
                }
            }

            var forwardMsg = messages.Select(msg => new CustomNode(ev.SenderInfo.Nick, ev.SenderInfo.UserId, msg));

            var (status, _, _) = await ev.SoraApi.SendGroupForwardMsg(ev.SourceGroup.Id, forwardMsg);

            if (status.RetCode != ApiStatusType.Ok)
            {
                await ev.Reply($"消息发送失败");
            }
        }
    }
}
