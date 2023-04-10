using Sora.Entities;
using Sora.Entities.Segment;
using SoraBot.BLL;
using SoraBot.Dto.Pixiv;
using SoraBot.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    public partial class Pixiv
    {
        private static async Task<MessageBody> DownPixivRank(Illust imagePage, Sora.EventArgs.SoraEvent.GroupMessageEventArgs ev)
        {
            if (imagePage.MetaPages.Length > 0)
            {
                // 多张图片
                var tasks = new List<Task<(string, string, object?)>>(imagePage.MetaPages.Length);
                var count = 0;
                var msg =   SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{imagePage.Id}\n")
                          + SoraSegment.Text($"标题：{imagePage.Title}\n")
                          + SoraSegment.Text($"标签：{Util.MakeTagString(imagePage.Tags)}\n")
                          + SoraSegment.Text($"作者：{imagePage.User.Name}\n"); ;
                foreach (var page in imagePage.MetaPages)
                {
                    if (count++ == 10)
                        break;

                    var url = Util.ChoiceGoodQualityImageUrl(page.ImageUrls);

                    if (string.IsNullOrEmpty(url))
                        continue;

                    tasks.Add(SeTuBll.DownloadPixivImageAsync(url));

                    var paths = await Task.WhenAll(tasks);
                    foreach (var path in paths)
                    {
                        if (string.IsNullOrEmpty(path.Item2))
                            continue;
                        msg += SoraSegment.Image(path.Item2);
                    }
                }
                return msg;
            }
            else
            {
                // 单张图片
                var url = imagePage.MetaSinglePage.OriginalImageUrl;

                if (string.IsNullOrEmpty(url))
                {
                    await ev.Reply("图片链接失踪了");
                    return null;
                }

                var path = await SeTuBll.DownloadPixivImageAsync(url);

                if (string.IsNullOrEmpty(path.Item2))
                {
                    await ev.Reply("图片文件失踪了");
                    return null;
                }

                var msg = SoraSegment.Text($"来源：https://www.pixiv.net/artworks/{imagePage.Id}\n")
                        + SoraSegment.Text($"标题：{imagePage.Title}\n")
                        + SoraSegment.Text($"标签：{Util.MakeTagString(imagePage.Tags)}\n")
                        + SoraSegment.Text($"作者：{imagePage.User.Name}\n")
                        + SoraSegment.Image(path.Item2);

                return msg;
            }
        }
    }
}
