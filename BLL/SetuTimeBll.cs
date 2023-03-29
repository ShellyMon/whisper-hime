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
using static System.Net.Mime.MediaTypeNames;

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
        internal static async Task<string> DownloadImageByAria(LoliconImageEntity image) {

            var url = image.Urls.Original
                .Replace("i.pixiv.cat", "i.pximg.net")
                .Replace("i.pixiv.re", "i.pximg.net");

            var savePath = Path.Combine(Environment.CurrentDirectory, "img", Path.GetFileName(image.Urls.Original));

            var status = await found.DownloadFileByAria(url, new Dictionary<String, Object>()
            {
                { "dir", System.Environment.CurrentDirectory + @"\img\"}
            });

            if (File.Exists(savePath))
            {
                using (var imgObj = SixLabors.ImageSharp.Image.Load(savePath))
                {
                    imgObj.Save(savePath, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });
                }
            }

            return status;
        }
    }
}
