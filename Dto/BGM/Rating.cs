using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SoraBot.Dto.BGM
{
    public class Rating
    {
        [JsonPropertyName("total")]
        public long Total { get; set; }

        [JsonPropertyName("count")]
        public Dictionary<string, long> Count { get; set; } = new();

        [JsonPropertyName("score")]
        public double Score { get; set; }
    }
}
