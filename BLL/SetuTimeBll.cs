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

        internal static async Task<string> DownloadImageByAria(LoliconImageEntity image)
        {
            var url = image.Urls.Original
                .Replace("i.pixiv.cat", "i.pximg.net")
                .Replace("i.pixiv.re", "i.pximg.net");

            var savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img", Path.GetFileName(image.Urls.Original));

            var options = new Dictionary<string, object>()
            {
                { "dir", Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img") }
            };

            var status = await ImageDownloadService.DownloadFileByAria(url, options);

            if (File.Exists(savePath))
            {
                using (var imgObj = Image.Load(savePath))
                {
                    await imgObj.SaveAsync(savePath, new PngEncoder() { CompressionLevel = PngCompressionLevel.Level9 });
                }
            }

            return status;
        }
    }
}
