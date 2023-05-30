using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace WhisperHime.Dto.soutubot
{
   

    public partial class Bot
    {
        [JsonProperty("data")]
        public Datum[] Data { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("factor")]
        public double Factor { get; set; }

        [JsonProperty("imageUrl")]
        public Uri ImageUrl { get; set; }

        [JsonProperty("searchOption")]
        public string SearchOption { get; set; }

        [JsonProperty("executionTime")]
        public double ExecutionTime { get; set; }
    }

    public partial class Datum
    {
        [JsonProperty("source")]
        public Source Source { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("language")]
        public Language Language { get; set; }

        [JsonProperty("pagePath")]
        public string PagePath { get; set; }

        [JsonProperty("subjectPath")]
        public string SubjectPath { get; set; }

        [JsonProperty("previewImageUrl")]
        public Uri PreviewImageUrl { get; set; }

        [JsonProperty("similarity")]
        public double Similarity { get; set; }
    }

    public enum Language { Cn, Gb, Jp };

    public enum Source { Ehentai, Nhentai, Panda };

    public partial class Bot
    {
        public static Bot FromJson(string json) => JsonConvert.DeserializeObject<Bot>(json);
    }

    public static class Serialize
    {
        public static string ToJson(this Bot self) => JsonConvert.SerializeObject(self);
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters = {
                LanguageConverter.Singleton,
                SourceConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class LanguageConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Language) || t == typeof(Language?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "cn":
                    return Language.Cn;
                case "gb":
                    return Language.Gb;
                case "jp":
                    return Language.Jp;
            }
            throw new Exception("Cannot unmarshal type Language");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Language)untypedValue;
            switch (value)
            {
                case Language.Cn:
                    serializer.Serialize(writer, "cn");
                    return;
                case Language.Gb:
                    serializer.Serialize(writer, "gb");
                    return;
                case Language.Jp:
                    serializer.Serialize(writer, "jp");
                    return;
            }
            throw new Exception("Cannot marshal type Language");
        }

        public static readonly LanguageConverter Singleton = new LanguageConverter();
    }

    internal class SourceConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Source) || t == typeof(Source?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "ehentai":
                    return Source.Ehentai;
                case "nhentai":
                    return Source.Nhentai;
                case "panda":
                    return Source.Panda;
            }
            throw new Exception("Cannot unmarshal type Source");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Source)untypedValue;
            switch (value)
            {
                case Source.Ehentai:
                    serializer.Serialize(writer, "ehentai");
                    return;
                case Source.Nhentai:
                    serializer.Serialize(writer, "nhentai");
                    return;
                case Source.Panda:
                    serializer.Serialize(writer, "panda");
                    return;
            }
            throw new Exception("Cannot marshal type Source");
        }

        public static readonly SourceConverter Singleton = new SourceConverter();
    }
}
