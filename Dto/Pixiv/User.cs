using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class User
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("account")]
        public string Account { get; set; } = string.Empty;

        [JsonPropertyName("profile_image_urls")]
        public ProfileImageUrls ProfileImageUrls { get; set; } = new();

        [JsonPropertyName("is_followed")]
        public bool IsFollowed { get; set; }
    }
}
