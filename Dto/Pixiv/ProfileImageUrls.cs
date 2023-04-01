using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class ProfileImageUrls
    {
        [JsonPropertyName("medium")]
        public string? Medium { get; set; }
    }
}
