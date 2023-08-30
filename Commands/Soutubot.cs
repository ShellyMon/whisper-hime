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
using WhisperHime.Dto.soutubot;

namespace WhisperHime.Commands
{
    [CommandSeries]
    public class Soutubot
    {

        [SoraCommand(CommandExpressions = new[] { "搜本" }, MatchType = Sora.Enumeration.MatchType.KeyWord, SourceType = MessageSourceMatchFlag.All)]
        public static async ValueTask GroupGetSoutubot(BaseMessageEventArgs ev)
        {
            var ImageList = ev.Message.GetAllImage();
            foreach (var item in ImageList)
            {
                var prefixUrl = ev.SourceType == SourceFlag.Private ? "https://images.weserv.nl/?url=" : "";
                var netWorkUrl = prefixUrl + item.Url;
                var path = await SoutubotBLL.DownloadSoutubotImageAsync(netWorkUrl, Path.GetFileName(Path.GetDirectoryName(netWorkUrl)));

                if (string.IsNullOrEmpty(path.Item2))
                {
                    await ev.Reply("图片文件失踪了");
                    return;
                }

                var SoutubotList = new Bot();
                try
                {
                    SoutubotList = await SoutubotBLL.RequestApiDataAsync(path.Item1, path.Item2);
                }
                catch (Exception)
                {
                    await SoutubotBLL.TaskAsync();
                    SoutubotList = await SoutubotBLL.RequestApiDataAsync(path.Item1, path.Item2);
                }

                
                var messages = new List<MessageBody>(SoutubotList.Data.Count());

                foreach (var res in SoutubotList.Data)
                {
                    if (res.Similarity > 40)
                    {
                        var PreviewPath = await SoutubotBLL.DownloadSoutubotImageAsync(res.PreviewImageUrl.AbsoluteUri, res.Source + res.Title + res.Language + res.Similarity);

                        var msg = SoraSegment.Text($"来源：{res.Source}\n")
                            + SoraSegment.Text($"标题：{res.Title}\n")
                            + SoraSegment.Text($"地址：https://nhentai.net{res.SubjectPath}\n")
                            + SoraSegment.Text($"语言：{res.Language}\n")
                            + SoraSegment.Text($"相似度：{res.Similarity}\n")
                            + SoraSegment.Image(PreviewPath.Item2);
                        messages.Add(msg);
                    }
                }
                if (messages.Count == 0)
                {

                    await ev.Reply("未查找到结果");
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
            }
        }

    }
}
