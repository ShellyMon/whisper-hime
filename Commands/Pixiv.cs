using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using SoraBot.BLL;
using SoraBot.Tools;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    /// <summary>
    /// Pixiv图片命令
    /// </summary>
    [CommandSeries]
    public class Pixiv
    {
        [SoraCommand(CommandExpressions = new[] { "^\\.pid (\\d+?)$" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Private, SuperUserCommand = false)]
        public static async ValueTask GetImageByPid(PrivateMessageEventArgs ev)
        {
            // 解析参数
            var match = ev.CommandRegex[0].Match(ev.Message.GetText());

            // 拿PID
            var pid = long.Parse(match.Groups[1].Value);
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
                var tasks = new List<Task<string>>(image.MetaPages.Length);
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
                    if (string.IsNullOrEmpty(path))
                        continue;
                    msg += SoraSegment.Image(path);
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

                if (string.IsNullOrEmpty(path))
                {
                    await ev.Reply("图片文件失踪了");
                    return;
                }

                var msg = SoraSegment.Text($"标题：{image.Title}\n")
                        + SoraSegment.Text($"标签：{Util.MakeTagString(image.Tags)}\n")
                        + SoraSegment.Text($"作者：{image.User.Name}\n")
                        + SoraSegment.Image(path);

                await ev.Reply(msg);
            }
        }
    }
}
