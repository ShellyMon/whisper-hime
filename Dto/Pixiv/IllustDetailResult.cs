using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class IllustDetailResult
    {
        [JsonPropertyName("illust")]
        public Illust? Illust { get; set; }
    }
}
