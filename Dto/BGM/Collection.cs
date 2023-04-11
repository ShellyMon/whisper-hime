using System.Text.Json.Serialization;

namespace SoraBot.Dto.BGM
{
    public class Collection
    {
        [JsonPropertyName("doing")]
        public long Doing { get; set; }
    }
}
