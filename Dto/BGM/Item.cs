using System;
using System.Text.Json.Serialization;

namespace WhisperHime.Dto.BGM
{
    public class Item
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public long Type { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("name_cn")]
        public string NameCn { get; set; } = string.Empty;

        [JsonPropertyName("summary")]
        public string Summary { get; set; } = string.Empty;

        [JsonPropertyName("air_date")]
        public string AirDate { get; set; } = string.Empty;

        [JsonPropertyName("air_weekday")]
        public long AirWeekday { get; set; }

        [JsonPropertyName("rating")]
        public Rating Rating { get; set; } = new();

        [JsonPropertyName("rank")]
        public long Rank { get; set; }

        [JsonPropertyName("images")]
        public Images Images { get; set; } = new();

        [JsonPropertyName("collection")]
        public Collection Collection { get; set; } = new();
    }
}
