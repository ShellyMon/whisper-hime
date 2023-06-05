using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhisperHime.Basics;
using WhisperHime.BLL;
using WhisperHime.Dto.BGM;

namespace WhisperHime.Commands
{
    [CommandSeries]
    public class Soutubot
    {

        [SoraCommand(CommandExpressions = new[] { "搜本" }, MatchType = Sora.Enumeration.MatchType.KeyWord, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupGetSoutubot(GroupMessageEventArgs ev)
        {
            var ImageList = ev.Message.GetAllImage();
            foreach (var item in ImageList)
            {
                var netWorkUrl = item.Url;
                var path = await SoutubotBLL.DownloadSoutubotImageAsync(netWorkUrl);

                if (string.IsNullOrEmpty(path.Item2))
                {
                    await ev.Reply("图片文件失踪了");
                    return;
                }

                var SoutubotList = await SoutubotBLL.RequestApiDataAsync(path.Item1, path.Item2);
                var messages = new List<MessageBody>(SoutubotList.Data.Count());

                foreach (var res in SoutubotList.Data)
                {
                    if (res.Similarity > 65)
                    {
                        var PreviewPath = await SoutubotBLL.DownloadSoutubotImageAsync(res.PreviewImageUrl.AbsoluteUri);

                        var msg = SoraSegment.Text($"来源：{res.Source}\n")
                            + SoraSegment.Text($"标题：{res.Title}\n")
                            + SoraSegment.Text($"语言：{res.Language}\n")
                            + SoraSegment.Text($"相似度：{res.Similarity}\n")
                            + SoraSegment.Text($"地址：https://nhentai.net{res.SubjectPath}\n")
                            + SoraSegment.Image(PreviewPath.Item2);
                        messages.Add(msg);
                    }
                }
                if (messages.Count == 0)
                {
                    await ev.SourceGroup.SendGroupMessage("未查找到结果");
                    return;
                }

                var forwardMsg = messages.Select(msg => new CustomNode(ev.SenderInfo.Nick, ev.SenderInfo.UserId, msg));

                var (status, _, _) = await ev.SourceGroup.SendGroupForwardMsg(forwardMsg);

                if (status.RetCode != ApiStatusType.Ok)
                {
                    await ev.SourceGroup.SendGroupMessage("消息发送失败");
                }

            }
        }

    }
}
