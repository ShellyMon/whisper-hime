using PixivClient.Models;
using System.Text.Json.Serialization;

namespace SoraBot.Dto
{
    public class IllustsResponse
    {
        [JsonPropertyName("illusts")]
        public Illust[] Illusts { get; set; } = Array.Empty<Illust>();

        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }
    }
}
