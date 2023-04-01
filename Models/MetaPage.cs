using System.Text.Json.Serialization;

namespace SoraBot.Models
{
    public class MetaPage
    {
        [JsonPropertyName("image_urls")]
        public ImageUrls ImageUrls { get; set; } = new();
    }
}
