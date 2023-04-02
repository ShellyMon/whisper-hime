using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SoraBot.Tools
{
    internal static class Extensions
    {
        /// <summary>
        /// 执行匹配并返回所有子句结果
        /// </summary>
        /// <param name="regex">正则表达式</param>
        /// <param name="input">输入</param>
        /// <returns></returns>
        internal static string[] MatchResult(this Regex regex, string input)
        {
            var match = regex.Match(input);

            if (match.Success)
                return match.Groups.Cast<Group>().Select(x => x.Value).ToArray();
            else
                return Array.Empty<string>();
        }
    }
}
