using System.Text.Json.Serialization;

namespace WhisperHime.Dto.Pixiv
{
    public class MetaPage
    {
        [JsonPropertyName("image_urls")]
        public ImageUrls ImageUrls { get; set; } = new();
    }
}
