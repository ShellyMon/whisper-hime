using System.Text.Json.Serialization;

namespace SoraBot.Models
{
    public class MetaSinglePage
    {
        [JsonPropertyName("original_image_url")]
        public string? OriginalImageUrl { get; set; }
    }
}
