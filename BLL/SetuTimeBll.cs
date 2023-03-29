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

        public static DataTable GetRandomImageFromDatabase(int num, string tag1, string tag2)
        {
            var query = DbScoped.Sugar.Queryable<information>();

            if (!string.IsNullOrEmpty(tag1))
            {
                query = query.Where(x => x.tags.Contains(tag1));
            }
            if (!string.IsNullOrEmpty(tag2))
            {
                query = query.Where(x => x.tags.Contains(tag2));
            }

            return query.Take(num)
                .OrderBy(x => SqlFunc.GetRandom())
                .ToDataTable();
        }

        internal static async Task<string> DownloadImageByAria(LoliconImageEntity image) {

            var url = image.Urls.Original
                .Replace("i.pixiv.cat", "i.pximg.net")
                .Replace("i.pixiv.re", "i.pximg.net");

            var savePath = Path.Combine(Environment.CurrentDirectory, "img", Path.GetFileName(image.Urls.Original));

            var status = await ImageDownloadService.DownloadFileByAria(url, new Dictionary<String, Object>()
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
