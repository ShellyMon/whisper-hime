using Microsoft.Extensions.Logging;
using SoraBot.Basics;
using SoraBot.Commands;
using SoraBot.Dto.Pixiv;
using SoraBot.PixivApi;
using System;
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
                var json = await pixiv.GetAsync(api);
                var obj = System.Text.Json.JsonSerializer.Deserialize<IllustDetailResult>(json);
                return obj?.Illust;
            }
            catch (Exception e)
            {
                logger.LogError(e, "Pixiv 接口调用失败");
                return null;
            }
        }
    }
}
