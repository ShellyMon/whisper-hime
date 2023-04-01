using System.Text.Json.Serialization;

namespace SoraBot.Models
{
    public class ProfileImageUrls
    {
        [JsonPropertyName("medium")]
        public string? Medium { get; set; }
    }
}
