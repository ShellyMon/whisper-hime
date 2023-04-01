﻿using System.Text.Json.Serialization;

namespace SoraBot.Models
{
    public class ImageUrls
    {
        [JsonPropertyName("square_medium")]
        public string? SquareMedium { get; set; }

        [JsonPropertyName("medium")]
        public string? Medium { get; set; }

        [JsonPropertyName("large")]
        public string? Large { get; set; }

        [JsonPropertyName("original")]
        public string? Original { get; set; }
    }
}
