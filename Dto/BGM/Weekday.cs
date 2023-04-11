using System.Text.Json.Serialization;

namespace SoraBot.Dto.BGM
{
    public class Weekday
    {
        [JsonPropertyName("en")]
        public string En { get; set; } = string.Empty;

        [JsonPropertyName("cn")]
        public string Cn { get; set; } = string.Empty;

        [JsonPropertyName("ja")]
        public string Ja { get; set; } = string.Empty;

        [JsonPropertyName("id")]
        public long Id { get; set; }
    }
}
