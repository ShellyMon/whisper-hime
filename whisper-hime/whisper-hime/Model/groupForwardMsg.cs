using Newtonsoft.Json;
using Message = Sisters.WudiLib.SendingMessage;

namespace whisper_hime.Model
{
    public class groupForwardMsg
    {
        [JsonProperty("group_id")]
        public long group_id { get; set; }
        [JsonProperty("messages")]
        public Message messages { get; set; }

        [JsonProperty("messages_id")]
        public long messages_id { get; set; }

        [JsonProperty("user_id")]
        public long user_id { get; set; }
    }
}
