using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SoraBot.Tools
{
    internal static class Util
    {
        /// <summary>
        /// 解析中文数值
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static int ParseChineseNumber(string text)
        {
            return text switch
            {
                "一" or "壹" => 1,
                "二" or "貳" or "两" or "俩" => 2,
                "三" or "叁" or "几" => 3,
                "四" or "肆" => 4,
                "五" or "伍" => 5,
                "六" or "陸" => 6,
                "七" or "柒" => 7,
                "八" or "捌" => 8,
                // 其它情況返回零不作处理
                _ => 0
            };
        }

        /// <summary>
        /// 判断图片大小是否超出可发送的限制
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        internal static bool IsImageTooLarge(string path)
        {
            // 25 MB
            return new FileInfo(path).Length > 0x1900000;
        }

        /// <summary>
        /// 生成逗号分隔的标签字符串
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        internal static string MakeTagString(IEnumerable<Dto.Pixiv.Tag> tags)
        {
            return string.Join('，', tags.Select(x => x.Name));
        }

        /// <summary>
        /// 选择质量最好的图片链接
        /// </summary>
        /// <param name="urls"></param>
        /// <returns></returns>
        internal static string ChoiceGoodQualityImageUrl(Dto.Pixiv.ImageUrls urls)
        {
            if (!string.IsNullOrEmpty(urls.Original))
                return urls.Original;
            if (!string.IsNullOrEmpty(urls.Large))
                return urls.Large;
            if (!string.IsNullOrEmpty(urls.Medium))
                return urls.Medium;
            if (!string.IsNullOrEmpty(urls.SquareMedium))
                return urls.SquareMedium;

            return string.Empty;
        }
    }
}
