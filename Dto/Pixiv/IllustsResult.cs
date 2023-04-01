using System;
using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class IllustsResult
    {
        [JsonPropertyName("illusts")]
        public Illust[] Illusts { get; set; } = Array.Empty<Illust>();

        [JsonPropertyName("next_url")]
        public string? NextUrl { get; set; }
    }
}
