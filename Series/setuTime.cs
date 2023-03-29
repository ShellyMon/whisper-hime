using Sora.Attributes.Command;
using Sora.Entities.Segment;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using SoraBot.Model;
using SqlSugar;
using SqlSugar.IOC;
using System.Data;
using System.Text.RegularExpressions;
using SoraBot.Basics;
using SoraBot.BLL;
using Sora.Entities.Segment.DataModel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Threading;

namespace SoraBot.Series
{
    [CommandSeries]
    public class setuTime
    {
        /// <summary>
        /// 私聊
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        [SoraCommand(CommandExpressions = new[] { "来(\\d)?[张|份|点](?:([^\\x00-\\xff]+)?)(?:\\s?)(?:([^\\x00-\\xff]+)?)([色|涩])图" },MatchType = Sora.Enumeration.MatchType.Regex,SourceType = SourceFlag.Private)]
        public async ValueTask setuCommand1(PrivateMessageEventArgs eventArgs)
        {
            MatchCollection mc = ImageDownloadService.setuRegexMatches(eventArgs.Message.ToString(), eventArgs.CommandRegex[0].ToString());
            GroupCollection groups = mc[0].Groups;
            DataTable RandomData = SetuTimeBll.setuTimeRandom(mc);
            if (groups[4].ToString() == "涩")
            {
                
                for (int i = 0; i < RandomData.Rows.Count; i++)
                {
                    var fileImage = System.Environment.CurrentDirectory + @"\imgs\" + System.IO.Path.GetFileName(RandomData.Rows[i]["imgUrls"].ToString());
                    eventArgs.IsContinueEventChain = false;
                    await eventArgs.Reply(SoraSegment.Image(fileImage));
                }
            }
        }
        /// <summary>
        /// 群聊
        /// </summary>
        /// <param name="eventArgs"></param>
        /// <returns></returns>
        [SoraCommand(CommandExpressions = new[] { "来(\\d)?[张|份|点](?:([^\\x00-\\xff]+)?)(?:\\s?)(?:([^\\x00-\\xff]+)?)([色|涩])图" },MatchType =Sora.Enumeration.MatchType.Regex, SourceType = SourceFlag.Group)]
        public async ValueTask setuCommand2(GroupMessageEventArgs eventArgs) {
            MatchCollection mc = ImageDownloadService.setuRegexMatches(eventArgs.Message.ToString(), eventArgs.CommandRegex[0].ToString());
            GroupCollection groups = mc[0].Groups;
            var customNodes = new List<CustomNode>();
            if (groups[4].ToString() == "涩")
            {//本地图库
                DataTable RandomData = SetuTimeBll.setuTimeRandom(mc);
                for (int i = 0; i < RandomData.Rows.Count; i++)
                {
                    var fileImage = System.Environment.CurrentDirectory + @"\imgs\" + System.IO.Path.GetFileName(RandomData.Rows[i]["imgUrls"].ToString());
                    var rows = RandomData.Rows[i];
                    customNodes.Add(new CustomNode("涩图人",eventArgs.Sender, $"www.pixiv.net/artworks/{rows["pid"].ToString()}\r\n title : {rows["title"].ToString()}\r\n 作者 : {rows["author"].ToString()}\r\n" +SoraSegment.Image(fileImage)));
                }
                var a = await eventArgs.SourceGroup.SendGroupForwardMsg(customNodes);
                if (a.apiStatus.ApiMessage== "timeout")
                {
                    await eventArgs.SourceGroup.SendGroupMessage("合并转发(群)消息发送失败");
                }
            }
            else//在线图库
            {
                string mun = "1";

                if (groups[1].ToString() != "")
                    mun = groups[1].ToString();

                var imageFetchResult = await ImageDownloadService.GetLoliconImage($"https://api.lolicon.app/setu/v2?&r18=0&excludeAI=true&num={mun}&tag={groups[2]}&tag={groups[3]}");

                if (imageFetchResult?.Data?.Count > 0)
                {
                    var downloadTasks = new List<Task<string>>();

                    foreach (var e in imageFetchResult.Data)
                    {
                        downloadTasks.Add(SetuTimeBll.DownloadImageByAria(e));
                    }

                    Task.WaitAll(downloadTasks.ToArray());

                    var msgNodes = new List<CustomNode>();

                    for (int i = 0; i < imageFetchResult.Data.Count; i++)
                    {
                        var image = imageFetchResult.Data[i];

                        if (downloadTasks[i].Result != "complete")
                            continue;

                        string savePath = Path.Combine(Environment.CurrentDirectory, "img", Path.GetFileName(image.Urls.Original));

                        msgNodes.Add(new CustomNode("涩图人", eventArgs.Sender, $"www.pixiv.net/artworks/{image.PID}\r\n title : {image.Title}\r\n 作者 : {image.Author}\r\n" + SoraSegment.Image(savePath)));
                    }

                    var (status, _, _) = await eventArgs.SourceGroup.SendGroupForwardMsg(msgNodes);

                    if (status.ApiMessage == "timeout")
                    {
                        await eventArgs.SourceGroup.SendGroupMessage("合并转发(群)消息发送失败");
                    }
                }
                else
                {
                    await eventArgs.SourceGroup.SendGroupMessage("未检索成功");
                    return;
                }
            }
        }
    } 
}
