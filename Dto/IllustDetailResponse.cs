using PixivClient.Models;
using System.Text.Json.Serialization;

namespace SoraBot.Dto
{
    public class IllustDetailResponse
    {
        [JsonPropertyName("illust")]
        public Illust? Illust { get; set; }
    }
}
