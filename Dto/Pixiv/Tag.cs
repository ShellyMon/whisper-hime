using System.Text.Json.Serialization;

namespace WhisperHime.Dto.Pixiv
{
    public class Tag
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("translated_name")]
        public string? TranslatedName { get; set; }
    }
}
