using Sora.Attributes.Command;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    /// <summary>
    /// Pixiv图片命令
    /// </summary>
    [CommandSeries]
    public class Pixiv
    {
        [SoraCommand(CommandExpressions = new[] { "" }, MatchType = MatchType.Regex, SourceType = SourceFlag.Private, SuperUserCommand = true)]
        public static async ValueTask GetImageByPid(PrivateMessageEventArgs ev)
        {
            await ev.Reply("Do");
        }
    }
}
