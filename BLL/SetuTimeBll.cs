using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SoraBot.Basics;
using SoraBot.Model;
using SqlSugar;
using SqlSugar.IOC;
using System.IO.Compression;
using YukariToolBox.LightLog;

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

            var saveDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache");
            var saveName = Path.GetFileName(image.Urls.Original);
            var fullPath = Path.Combine(saveDirPath, saveName);

            // 检查缓存
            if (!File.Exists(fullPath))
            {
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
            }

            if (File.Exists(fullPath))
            {
                Log.Info(nameof(DownloadImageByAriaAsync), $"Recompress image: {fullPath}");

                // 重新压缩图片，改变HASH
                using (var imgObj = await Image.LoadAsync(fullPath))
                {
                    var encoder = new PngEncoder() { CompressionLevel = PngCompressionLevel.BestSpeed };
                    await imgObj.SaveAsync(fullPath, encoder);
                }

                Log.Info(nameof(DownloadImageByAriaAsync), $"Finished");

                return fullPath;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
