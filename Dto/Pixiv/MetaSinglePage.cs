using System.Text.Json.Serialization;

namespace WhisperHime.Dto.Pixiv
{
    public class MetaSinglePage
    {
        [JsonPropertyName("original_image_url")]
        public string? OriginalImageUrl { get; set; }
    }
}
