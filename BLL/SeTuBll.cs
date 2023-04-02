using LiteDB;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SoraBot.Basics;
using SoraBot.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SoraBot.BLL
{
    /// <summary>
    /// 色图
    /// </summary>
    internal class SeTuBll
    {
        internal static List<Illustration> GetRandomImageFromDatabase(int num, bool adult, string tag1, string tag2)
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

            if (!adult)
            {
                query = query.Where(x => !x.IsAdult);
            }

            var expr = BsonExpression.Create("RANDOM()");

            query = query.OrderBy(expr);

            return query.Limit(num).ToList();
        }

        internal static async Task<(string, string, object?)> DownloadPixivImageAsync(string url, object? tag = null)
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
                    logger.LogError("图片下载失败");
                    return (url, string.Empty, tag);
                }
            }

            if (File.Exists(fullPath))
            {
                try
                {
                    logger.LogInformation("压缩图片 {}", fullPath);

                    // 重新压缩图片，改变HASH
                    using (var imgObj = await Image.LoadAsync(fullPath))
                    {
                        var encoder = new PngEncoder() { CompressionLevel = PngCompressionLevel.BestSpeed };
                        await imgObj.SaveAsync(fullPath, encoder);
                    }

                    logger.LogInformation("完成");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "图片压缩失败");
                }

                return (url, fullPath, tag);
            }
            else
            {
                logger.LogError("文件不存在");
                return (url, string.Empty, tag);
            }
        }
    }
}
