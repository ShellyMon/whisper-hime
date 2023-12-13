using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace WhisperHime.Tools
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
                "九" or "玖" => 9,
                "十" or "拾" => 10,
                // 其它情況返回零不作处理
                _ => 0
            };
        }

        /// <summary>
        /// 解析排行榜配置
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        internal static string ParseChineseRanking(string text)
        {
            return text switch
            {
                "月" or "m" or "M" => "month",
                "周" or "w" or "W"=> "week",
                "日" or "d" or "D" => "day",
                "原创" or "wo" or "WO" => "week_original",
                "新人" or "wr" or "WR" => "week_rookie",
                // 其它情況返回零不作处理
                _ => "month"
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
        ///计算soutubot的ApiKey
        /// </summary>
        /// <param name="key1">时间戳</param>
        /// <param name="key2">UA长度</param>
        /// <returns></returns>
        internal static string ComputeApiKey(long key1, long key2)
        {
            var key = Convert.ToInt64(Math.Pow(key1, 2) + Math.Pow(key2, 2) + 1647857448721);
            key -= key % 100;
            return new string(Convert.ToBase64String(Encoding.ASCII.GetBytes(key.ToString())).TakeWhile(x => x != '=').Reverse().ToArray());
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

        public static List<string> GetFileName(string path)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileInfo[] finfo = dir.GetFiles();
                List<string> fnames = new List<string>();
                for (int i = 0; i < finfo.Length; i++)
                {
                    fnames.Add(finfo[i].Name);
                }
                return fnames;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static string DayOfWeek()
        {
            string week = string.Empty;
            string enweek = DateTime.Today.DayOfWeek.ToString();
            switch (enweek)
            {
                case "Monday":
                    week = "星期一";
                    break;
                case "Tuesday":
                    week = "星期二";
                    break;
                case "Wednesday":
                    week = "星期三";
                    break;
                case "Thursday":
                    week = "星期四";
                    break;
                case "Friday":
                    week = "星期五";
                    break;
                case "Saturday":
                    week = "星期六";
                    break;
                case "Sunday":
                    week = "星期日";
                    break;
            }
            return week;
        }
    }
}
