using Microsoft.Extensions.Logging;
using SoraBot.Basics;
using SoraBot.Commands;
using SoraBot.Dto.Pixiv;
using SoraBot.PixivApi;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SoraBot.BLL
{
    /// <summary>
    /// Pixiv
    /// </summary>
    internal class PixivBll
    {
        internal static async Task<Illust?> GetImageByPidAsync(long pid)
        {
            var logger = Ioc.Require<ILogger<Pixiv>>();

            try
            {
                var pixiv = Ioc.Require<PixivClient>();
                var api = $"illust/detail?illust_id={pid}";
                var json = await pixiv.GetAsync(api, 3);
                var obj = System.Text.Json.JsonSerializer.Deserialize<IllustDetailResult>(json);
                return obj?.Illust;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Pixiv 接口调用失败");
                return null;
            }
        }

        /// <summary>
        /// day: 日榜, week: 周榜, month: 月榜, day_male: 男性日榜, day_female: 女性日榜, 
        /// week_original: 周原创榜, week_rookie: 周新人榜, 
        /// day_r18: R18 日榜, day_male_r18: 男性 R18 日榜, day_female_r18: 女性 R18 日榜
        /// </summary>
        /// <param name="RankType"></param>
        /// <returns></returns>
        internal static async Task<List<Illusts>?> GetImageByRankingAsync(string RankType)
        {
            var logger = Ioc.Require<ILogger<Pixiv>>();
            try
            {
                var pixiv = Ioc.Require<PixivClient>();
                var api = $"illust/ranking?mode={RankType}";
                var json = await pixiv.GetAsync(api, 3);
                var obj = System.Text.Json.JsonSerializer.Deserialize<IllustsDetailResult>(json);
                return obj?.Illusts;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Pixiv 接口调用失败");
                return null;
            }
        }
    }
}
