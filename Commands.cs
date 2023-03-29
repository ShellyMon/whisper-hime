using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;

namespace SoraBot
{
    [CommandSeries]
    public class Commands
    {
        /// <summary>
        /// 随便写的测试
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        [SoraCommand(CommandExpressions = new[] { "好耶" }, MatchType = Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public async ValueTask testCommand1(GroupMessageEventArgs eventArgs) {
            
            eventArgs.IsContinueEventChain = false;
            await eventArgs.Reply("好耶好耶！");
            var customNodes = new List<CustomNode>();
            customNodes.Add(new CustomNode("涩图人", eventArgs.Sender,SoraSegment.Image("D:\\qqboot\\sora\\SoraBot\\bin\\Debug\\net6.0\\imgs\\95527516_p0.jpg")));
            customNodes.Add(new CustomNode("涩图人", eventArgs.Sender, SoraSegment.Image("D:\\qqboot\\sora\\SoraBot\\bin\\Debug\\net6.0\\imgs\\96029563_p0.jpg")));
            await eventArgs.SourceGroup.SendGroupForwardMsg(customNodes);
        }

        [SoraCommand(CommandExpressions = new[] { "好耶" }, SourceType = SourceFlag.Private)]
        public async ValueTask testCommand2(PrivateMessageEventArgs eventArgs)
        {
            //var a = DbScoped.Sugar.Ado.GetDataTable("select * from information");
            //DbScoped.Sugar.DbFirst.IsCreateAttribute().CreateClassFile("c:\\Demo\\5", "Models");
            eventArgs.IsContinueEventChain = false;
            await eventArgs.Reply("好耶好耶！");
        }
    }
}
