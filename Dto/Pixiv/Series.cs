using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class Series
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;
    }
}
