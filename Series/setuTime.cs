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
            MatchCollection mc = found.setuRegexMatches(eventArgs.Message.ToString(), eventArgs.CommandRegex[0].ToString());
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
            MatchCollection mc = found.setuRegexMatches(eventArgs.Message.ToString(), eventArgs.CommandRegex[0].ToString());
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
                string gettext = found.GetResponseString(found.CreateGetHttpResponse($"https://api.lolicon.app/setu/v2?&r18=0&excludeAI=true&num={mun}&tag={groups[2]}&tag={groups[3]}"));
                JObject list = JObject.Parse(gettext);
                if (list["data"].ToString() == "[]")
                {
                    await eventArgs.SourceGroup.SendGroupMessage("未检索成功");
                    return;
                }
                else
                {
                    List<Task<string>> thread = new List<Task<string>>();
                    var customNodes2 = new List<CustomNode>();
                    foreach (var listss in list["data"])
                    {
                        thread.Add(SetuTimeBll.ReciveMsg(listss));
                    }
                    Task.WaitAll(thread.ToArray());
                    if (thread[0].Result == "error")
                    {
                        await eventArgs.SourceGroup.SendGroupMessage(eventArgs.Sender.At()+ "未检索成功");
                        return;
                    }
                    for (int i = 0; i < list["data"].Count(); i++)
                    {
                        var cd = list["data"][i];
                        if (thread[i].Result == "false")
                            break;
                        string saveUrl = System.Environment.CurrentDirectory + @"\img\" + System.IO.Path.GetFileName(cd["urls"]["original"].ToString());
                        customNodes2.Add(new CustomNode("涩图人", eventArgs.Sender, $"www.pixiv.net/artworks/{cd["pid"].ToString()}\r\n title : {cd["title"].ToString()}\r\n 作者 : {cd["author"].ToString()}\r\n" + SoraSegment.Image(saveUrl)));
                    }
                    var a = await eventArgs.SourceGroup.SendGroupForwardMsg(customNodes2);
                    if (a.apiStatus.ApiMessage == "timeout")
                    {
                        await eventArgs.SourceGroup.SendGroupMessage("合并转发(群)消息发送失败");
                    }
                }
            }
        }
    } 
}
