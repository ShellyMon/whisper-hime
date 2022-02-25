using Newtonsoft.Json;

namespace whisper_hime
{
    public class getImage
    {
        [JsonProperty("file")]
        public string file { get; set; }

        [JsonProperty("size")]
        public string size { get; set; }

        [JsonProperty("filename")]
        public string filename { get; set; }

        [JsonProperty("url")]
        public string url { get; set; }
    }
}
