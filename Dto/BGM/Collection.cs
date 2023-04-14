using System.Text.Json.Serialization;

namespace WhisperHime.Dto.BGM
{
    public class Collection
    {
        [JsonPropertyName("doing")]
        public long Doing { get; set; }
    }
}
