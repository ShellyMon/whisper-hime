using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class MetaPage
    {
        [JsonPropertyName("image_urls")]
        public ImageUrls ImageUrls { get; set; } = new();
    }
}
