using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class ImageUrls
    {
        [JsonPropertyName("square_medium")]
        public string? SquareMedium { get; set; } = string.Empty;

        [JsonPropertyName("medium")]
        public string? Medium { get; set; } = string.Empty;

        [JsonPropertyName("large")] 
        public string? Large { get; set; } = string.Empty;

        [JsonPropertyName("original")] 
        public string? Original { get; set; } = string.Empty;
    }
}
