using System.Text.Json.Serialization;

namespace WhisperHime.Dto.Pixiv
{
    public class ProfileImageUrls
    {
        [JsonPropertyName("medium")]
        public string? Medium { get; set; }
    }
}
