using System;
using System.Text.Json.Serialization;

namespace SoraBot.Dto.BGM
{
    internal class CalendarItem
    {
        [JsonPropertyName("weekday")]
        public Weekday Weekday { get; set; } = new();

        [JsonPropertyName("items")]
        public Item[] Items { get; set; } = Array.Empty<Item>();
    }
}
