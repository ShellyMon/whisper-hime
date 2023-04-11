using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using SoraBot.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace SoraBot.Commands
{
    [CommandSeries]
    public class BGM
    {
        [SoraCommand(CommandExpressions = new[] { "每日放送" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupGetBGManime(GroupMessageEventArgs ev)
        {
            var result = await ImageDownloadService.GetBgmAnimeCalendar();

            var messages = new List<MessageBody>();

            foreach (var weekMsg in result) {

                var weekDayCn = weekMsg.Weekday.Cn;

                
                if (weekMsg.Weekday.Id == (long)DateTime.Now.DayOfWeek)
                {
                    messages.Add(weekDayCn);

                    foreach (var weekItems in weekMsg.Items)
                    {
                        if (weekItems.Images == null)
                            continue;

                        var msg = SoraSegment.Text($"番名：{weekItems.Name}\n")
                                 + SoraSegment.Text($"中文名：{weekItems.NameCn}\n")
                                 + SoraSegment.Text($"详细信息：{weekItems.Url}\n")
                                 + SoraSegment.Image(weekItems.Images.Large)
                                 + SoraSegment.Text($"\n放送开始：{weekItems.AirDate}\n")
                                 + SoraSegment.Text($"BGM评分：{weekItems.Rating.Score}");
                        messages.Add(msg);
                    }
                }
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
