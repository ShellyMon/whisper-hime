using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace WhisperHime.Dto.Pixiv
{
    public class IllustsDetailResult
    {
        [JsonPropertyName("illusts")]
        public List<Illust>? Illusts { get; set; }
    }
}
