using LiteDB;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SoraBot.Basics;
using SoraBot.Entity;

namespace SoraBot.BLL
{
    /// <summary>
    /// 色图
    /// </summary>
    internal class SeTuBll
    {
        internal static List<Illustration> GetRandomImageFromDatabase(int num, string tag1, string tag2)
        {
            var db = Ioc.Require<ILiteDatabase>();
            var query = db.GetCollection<Illustration>().Query();

            if (!string.IsNullOrEmpty(tag1))
            {
                query = query.Where(x => x.Tags.Contains(tag1));
            }
            if (!string.IsNullOrEmpty(tag2))
            {
                query = query.Where(x => x.Tags.Contains(tag2));
            }

            var expr = BsonExpression.Create("RANDOM()");

            query = query.OrderBy(expr);

            return query.Limit(num).ToList();
        }

        internal static async Task<string> DownloadPixivImageAsync(string url)
        {
            var logger = Ioc.Require<ILogger<SeTuBll>>();

            var url2 = url
                .Replace("i.pixiv.cat", "i.pximg.net")
                .Replace("i.pixiv.re", "i.pximg.net");

            var saveDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "img-cache");
            var saveName = Path.GetFileName(url);
            var fullPath = Path.Combine(saveDirPath, saveName);

            // 检查缓存
            if (!File.Exists(fullPath))
            {
                var options = new Dictionary<string, object>()
                {
                    { "dir", saveDirPath },
                    { "out", saveName },
                };

                var status = await ImageDownloadService.DownloadFileByAriaAsync(url2, options);

                if (status != "complete")
                {
                    return string.Empty;
                }
            }

            if (File.Exists(fullPath))
            {
                logger.LogInformation("Recompress image: {fullPath}", fullPath);

                // 重新压缩图片，改变HASH
                using (var imgObj = await Image.LoadAsync(fullPath))
                {
                    var encoder = new PngEncoder() { CompressionLevel = PngCompressionLevel.BestSpeed };
                    await imgObj.SaveAsync(fullPath, encoder);
                }

                logger.LogInformation("Finished");

                return fullPath;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
