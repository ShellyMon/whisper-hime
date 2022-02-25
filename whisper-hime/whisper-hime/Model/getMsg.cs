using Newtonsoft.Json;

namespace whisper_hime.Model
{
    public class getMsg
    {
        [JsonProperty("message_id")]
        public long message_id { get; set; }
        [JsonProperty("real_id")]
        public long real_id { get; set; }
        [JsonProperty("sender")]
        public object? sender { get; set; }
        [JsonProperty("Time")]
        public long Time { get; set; }
        [JsonProperty("message")]
        public object? message { get; set; }
        [JsonProperty("raw_message")]
        public string? raw_message { get; set; }
    }
}
