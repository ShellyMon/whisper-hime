using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Tools
{
    internal static class Util
    {
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

        internal static bool IsImageTooLarge(string path)
        {
            // 25 MB
            return new FileInfo(path).Length > 0x1900000;
        }
    }
}
