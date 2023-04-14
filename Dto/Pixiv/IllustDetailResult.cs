using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace WhisperHime.Dto.Pixiv
{
    public class IllustDetailResult
    {
        [JsonPropertyName("illust")]
        public Illust? Illust { get; set; }


    }
}
