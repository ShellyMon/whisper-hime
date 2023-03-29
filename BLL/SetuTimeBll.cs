using SqlSugar.IOC;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SoraBot.Model;
using Newtonsoft.Json.Linq;
using SoraBot.Basics;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp;
using System.IO;

namespace SoraBot.BLL
{
    public class SetuTimeBll
    {
        public SetuTimeBll() { }
        public static DataTable setuTimeRandom(MatchCollection mc) {
            GroupCollection groups = mc[0].Groups;
            string tag1 = groups[2].ToString();
            string tag2 = groups[3].ToString();
            return DbScoped.Sugar.Queryable<information>()
            .Where(it => it.tags.Contains(tag1) && it.tags.Contains(tag2))
            .Take(int.Parse(groups[1].ToString()))
            .OrderBy(st => SqlFunc.GetRandom())
            .ToDataTable();
        }
        public static async Task<string> ReciveMsg(JToken listss) {

            string tags = listss["tags"].ToString().Replace("\r\n", "");
            string httpUrl = listss["urls"]["original"].ToString().Replace("i.pixiv.cat", "i.pximg.net").Replace("i.pixiv.re", "i.pximg.net");
            string saveUrl = System.Environment.CurrentDirectory + @"\img\" + System.IO.Path.GetFileName(httpUrl);
            var cc = await found.aria(httpUrl, new Dictionary<String, Object>()
            {
                { "dir", System.Environment.CurrentDirectory + @"\img\"}
            });
            if (Directory.Exists(saveUrl))
            {
                using (var image = SixLabors.ImageSharp.Image.Load(saveUrl))
                {
                    image.Save(saveUrl, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });
                }
            }
            return cc;
        }
    }
}
