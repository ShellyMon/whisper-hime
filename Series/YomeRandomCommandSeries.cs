using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using SoraBot.BLL;

namespace SoraBot.Series
{
    [CommandSeries]
    public class YomeRandomCommandSeries
    {
        [SoraCommand(CommandExpressions = new[] { "抽老婆" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public async ValueTask GroupGetTsuma(GroupMessageEventArgs eventArgs)
        {
            long SendUserId = eventArgs.SenderInfo.UserId;
            //获取数据
            var DarlingRomdum = await YomeRandomBll.GetDarlingRomdunDay(SendUserId.ToString());
            //拼接地址
            var saveDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"clp\img");
            var saveName = Path.GetFileName(DarlingRomdum.path);
            var fullPath = Path.Combine(saveDirPath, saveName);

            await eventArgs.SourceGroup.SendGroupMessage(SoraSegment.At(SendUserId)+$"さんが二次元で結婚するであろうヒロインは\r\n"+ SoraSegment.Image(fullPath)+$"\r\n【{DarlingRomdum.Name}】{DarlingRomdum.YL}");

        }
    }
}
