using System.Text.Json.Serialization;

namespace WhisperHime.Dto.BGM
{
    public class Images
    {
        [JsonPropertyName("large")]
        public string Large { get; set; } = string.Empty;

        [JsonPropertyName("common")]
        public string Common { get; set; } = string.Empty;

        [JsonPropertyName("medium")]
        public string Medium { get; set; } = string.Empty;

        [JsonPropertyName("small")]
        public string Small { get; set; } = string.Empty;

        [JsonPropertyName("grid")]
        public string Grid { get; set; } = string.Empty;
    }
}
