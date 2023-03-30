using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SoraBot.Basics;
using SoraBot.Model;
using SqlSugar;
using SqlSugar.IOC;

namespace SoraBot.BLL
{
    public class SetuTimeBll
    {
        public static List<information> GetRandomImageFromDatabase(int num, string tag1, string tag2)
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

            return query.OrderBy(x => SqlFunc.GetRandom())
                .Take(num)
                .ToList();
        }

        internal static async Task<string> DownloadImageByAriaAsync(LoliconImageEntity image)
        {
            var url = image.Urls.Original
                .Replace("i.pixiv.cat", "i.pximg.net")
                .Replace("i.pixiv.re", "i.pximg.net");

            var saveDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img");
            var saveName = Path.GetFileName(image.Urls.Original);

            var options = new Dictionary<string, object>()
            {
                { "dir", saveDirPath },
                { "out", saveName },
            };

            var status = await ImageDownloadService.DownloadFileByAriaAsync(url, options);

            if (status != "complete")
            {
                return string.Empty;
            }

            var fullPath = Path.Combine(saveDirPath, saveName);

            if (File.Exists(fullPath))
            {
                // 重新压缩图片，改变HASH
                using (var imgObj = await Image.LoadAsync(fullPath))
                {
                    var encoder = new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 };
                    await imgObj.SaveAsync(fullPath, encoder);
                }
            }

            return fullPath;
        }
    }
}
