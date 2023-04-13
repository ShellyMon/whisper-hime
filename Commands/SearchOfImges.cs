using Sora.Attributes.Command;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using SoraBot.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Commands
{
    [CommandSeries]
    public class SearchOfImges
    {

        // 来点色图（群聊）
        [SoraCommand(CommandExpressions = new[] { ".搜图" }, MatchType = Sora.Enumeration.MatchType.KeyWord, SourceType = SourceFlag.Group)]
        public static async ValueTask GroupGetSearch(GroupMessageEventArgs ev)
        {
            var b = ev.Message.GetAllImage();
            foreach (var item in b)
            {
                var c =  await SauceClientService.SauceResult(item.Url);
                foreach (var d in c.Results)
                {
                    //d.Similarity;
                }
            }
        }

    }
}
