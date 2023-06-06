using LiteDB;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using WhisperHime.Basics;
using WhisperHime.Entity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.ColorSpaces;

namespace WhisperHime.BLL
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
                query = query.Where(x => x.Tags.Contains(tag1) || x.Title.Contains(tag1) || x.Artist.Contains(tag1));
            }
            if (!string.IsNullOrEmpty(tag2))
            {
                query = query.Where(x => x.Tags.Contains(tag2) || x.Title.Contains(tag2) || x.Artist.Contains(tag2));
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

                await ImgCompress(fullPath);
            }

            if (File.Exists(fullPath))
            {
                return (url, fullPath, tag);
            }
            else
            {
                logger.LogError("文件不存在");
                return (url, string.Empty, tag);
            }
        }

        internal static async Task ImgCompress(string fullPath)
        {
            var logger = Ioc.Require<ILogger<SeTuBll>>();

            if (File.Exists(fullPath))
            {
                try
                {
                    logger.LogInformation("压缩图片 {}", fullPath);

                    // 重新压缩图片，改变HASH
                    using (var imgObj = await Image.LoadAsync(fullPath))
                    {
                        var encoder = new PngEncoder() { CompressionLevel = PngCompressionLevel.Level2 };
                        var BitsPerPixel = imgObj.PixelType.BitsPerPixel;

                        if (BitsPerPixel == 24)
                        {
                            var imgRgb24 = (Image<Rgb24>)imgObj;
                            imgRgb24[0, 0] = new Rgb24((byte)(imgRgb24[0, 0].R - 1 < 0 ? 0 : 0), 0xFF, 0xFF);
                            await imgRgb24.SaveAsync(fullPath, encoder);
                        }

                        if (BitsPerPixel == 32)
                        {
                            var imgRgba32 = (Image<Rgba32>)imgObj;
                            imgRgba32[0, 0] = new Rgba32((byte)(imgRgba32[0, 0].R - 1 < 0 ? 0 : 0), 0xFF, 0xFF, 100);
                            await imgRgba32.SaveAsync(fullPath, encoder);
                        }

                        await imgObj.SaveAsync(fullPath, encoder);
                    }

                    logger.LogInformation("完成");
                }
                catch (Exception e)
                {
                    logger.LogError(e, "图片压缩失败");
                }
            }
        }
    }
}
