using MonkeyCache.LiteDB;
using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using SoraBot.BLL;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    /// <summary>
    /// 抽老婆
    /// </summary>
    [CommandSeries]
    public class WifeGacha
    {
        [SoraCommand(CommandExpressions = new[] { "抽老婆" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupWifeGacha(GroupMessageEventArgs ev)
        {
            var date = Barrel.Current.Get<DateTime>($"User.WifeGachaDate.{ev.Sender.Id}");

            if (date == DateTime.Now.Date)
            {
                await ev.Reply("新的老婆，明天再抽吧！");
                return;
            }

            var wife = WifeGachaBll.DoWifeGacha();

            if (wife == null)
            {
                await ev.Reply("你的老婆跟别人跑了");
                return;
            }

            var imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, wife.ImageUrl);

            if (!File.Exists(imagePath))
            {
                await ev.Reply("你的老婆的照片被吃了");
                return;
            }

            Barrel.Current.Add($"User.WifeGachaDate.{ev.Sender.Id}", DateTime.Now.Date, TimeSpan.FromDays(1));

            var msg = SoraSegment.At(ev.Sender)
                    + wife.Sentence
                    + SoraSegment.Image(imagePath);

            await ev.Reply(msg);
        }
    }
}
