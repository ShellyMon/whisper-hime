using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperHime.Dto.soutubot
{
    public partial class SoutuBotMods
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("solution")]
        public Solution Solution { get; set; }

        [JsonProperty("startTimestamp")]
        public long StartTimestamp { get; set; }

        [JsonProperty("endTimestamp")]
        public long EndTimestamp { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Solution
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("status")]
        public long Status { get; set; }

        [JsonProperty("cookies")]
        public Cooky[] Cookies { get; set; }

        [JsonProperty("userAgent")]
        public string UserAgent { get; set; }

        [JsonProperty("headers")]
        public Headers Headers { get; set; }
    }

    public partial class Cooky
    {
        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("expiry")]
        public long Expiry { get; set; }

        [JsonProperty("httpOnly")]
        public bool HttpOnly { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("sameSite")]
        public string SameSite { get; set; }

        [JsonProperty("secure")]
        public bool Secure { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
    public partial class Headers
    {
    }

    public partial class SoutuBotMods
    {
        public static SoutuBotMods FromJson(string json) => JsonConvert.DeserializeObject<SoutuBotMods>(json, WhisperHime.Dto.soutubot.Converter.Settings);
    }
}
