using SauceNET.Model;
using Sora.Attributes.Command;
using Sora.Entities;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace WhisperHime.Commands
{
    [CommandSeries]
    public class ToolBox
    {
        [SoraCommand(CommandExpressions = new[] { "工具箱" }, MatchType = Sora.Enumeration.MatchType.Full, SourceType = MessageSourceMatchFlag.All)]
        public static async ValueTask ToolBoxSetGetlocal(BaseMessageEventArgs ev)
        {
            var messages = new List<MessageBody>() 
            {
                {
                    SoraSegment.Image("http://q1.qlogo.cn/g?b=qq&nk=865336759&s=100")+
                    SoraSegment.Text("来自🍀天河🍀佬的馈赠\n")+
                    SoraSegment.Text("  动漫花园吧养老动漫交流α群（涩图相册）\n")+
                    SoraSegment.Text("      https://tianhe.myds.me:5001/mo/request/N6lbgZ89P  上传\n")+
                    SoraSegment.Text("      https://tianhe.myds.me:5001/mo/sharing/9SvPe1sUm  下载  \n")+
                    SoraSegment.Text("  动漫花园吧养老动漫交流α群（壁纸）\n")+
                    SoraSegment.Text("      https://tianhe.myds.me:5001/mo/request/YxeS3gW5d  上传 \n")+
                    SoraSegment.Text("      https://tianhe.myds.me:5001/mo/sharing/BEWcYsgXE  下载\n")+
                    SoraSegment.Text("---密码：147258----")
                },
                {
                    SoraSegment.Image("http://q1.qlogo.cn/g?b=qq&nk=867258173&s=100")+
                    SoraSegment.Text("来自禦滝尹之介[博爱党]佬的馈赠\n")+
                    SoraSegment.Text("★★★★★★★ 新人必看资源导航 ★★★★★★\n")+
                    SoraSegment.Text("  http://867258173.ys168.com/\n\n")+
                    SoraSegment.Text("★★★动漫花园吧 Bangumi 公用账号★★★\n")+
                    SoraSegment.Text("  账号: 393957253@qq.com\n")+
                    SoraSegment.Text("  密码: dmhybgxzh\n")+
                    SoraSegment.Text("  昵称: 二次元の神\n\n")+
                    SoraSegment.Text("✨bangumi多标签搜索网站✨\n")+
                    SoraSegment.Text("  https://cityhunter.me/\n\n")+
                    SoraSegment.Text("✨花园群统计-观看新番选择哪个片源的搬运组是无修正版统计表✨\n")+
                    SoraSegment.Text("  https://docs.qq.com/sheet/DRWVVdVhuSHlXeEd5?tab=BB08J2")+
                    SoraSegment.Text("✨✨✨ 表番历代年鉴查询网站 ✨✨✨\n")+
                    SoraSegment.Text("  ①アニメ大全 日本整合的年鉴合集,可以通过年代/五十音来查询,新番也会添加里面 ←推荐 \n  https://dwz.date/fuPT\n")+
                    SoraSegment.Text("  ② 维基百科，国外最全科普网站 ←推荐\n   https://dwz.date/fuPV\n从1960-如今年代番名和具体播放时间都有详细记载，推荐\n")+
                    SoraSegment.Text("  ③ bgm，国内最全ACGN资讯站点 ←推荐\n   https://bangumi.tv/anime/browser/airtime/1969\n")+
                    SoraSegment.Text("  其他域名\n https://​chii.in/\n https://bgm.tv/\n")+
                    SoraSegment.Text("  ④ 另外欧美站点，AniDB和MAL也可以互补列表的英文名，罗马音\n https://anidb.net\n https://myanimelist.net/  \n")
                },
                { 
                    SoraSegment.Text("---在线番剧链接---\n")+
                    SoraSegment.Text("  AGE动漫：https://www.agemys.com\n")+
                    SoraSegment.Text("  次元城动漫：https://www.cycdm01.top\n")+
                    SoraSegment.Text("  上述网站存在一定的广告，尤其是次元城和OmoFun，但是能用\n")+
                    SoraSegment.Text("---待补充---")
                },
                {
                    SoraSegment.Text("---以图搜图分类---\n")+
                    SoraSegment.Text("  Yandex：https://yandex.com/images\n")+
                    SoraSegment.Text("  搜图bot酱：https://soutubot.moe\n")+
                    SoraSegment.Text("  trace：https://trace.moe\n")+
                    SoraSegment.Text("  saucenao：https://saucenao.com\n")+
                    SoraSegment.Text("---待补充---")
                },
                {
                    SoraSegment.Text("---欢迎投稿哦~---\n")
                },

            };

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
