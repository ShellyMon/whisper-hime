using System;
using System.Text.Json.Serialization;

namespace SoraBot.Dto.Pixiv
{
    public class Illust
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("image_urls")]
        public ImageUrls ImageUrls { get; set; } = new();

        [JsonPropertyName("caption")]
        public string Caption { get; set; } = string.Empty;

        [JsonPropertyName("restrict")]
        public long Restrict { get; set; }

        [JsonPropertyName("user")]
        public User User { get; set; } = new();

        [JsonPropertyName("tags")]
        public Tag[] Tags { get; set; } = Array.Empty<Tag>();

        [JsonPropertyName("tools")]
        public string[] Tools { get; set; } = Array.Empty<string>();

        [JsonPropertyName("create_date")]
        public DateTimeOffset CreateDate { get; set; }

        [JsonPropertyName("page_count")]
        public long PageCount { get; set; }

        [JsonPropertyName("width")]
        public long Width { get; set; }

        [JsonPropertyName("height")]
        public long Height { get; set; }

        [JsonPropertyName("sanity_level")]
        public long SanityLevel { get; set; }

        [JsonPropertyName("x_restrict")]
        public long XRestrict { get; set; }

        [JsonPropertyName("series")]
        public Series? Series { get; set; }

        [JsonPropertyName("meta_single_page")]
        public MetaSinglePage MetaSinglePage { get; set; } = new();

        [JsonPropertyName("meta_pages")]
        public MetaPage[] MetaPages { get; set; } = Array.Empty<MetaPage>();

        [JsonPropertyName("total_view")]
        public long TotalView { get; set; }

        [JsonPropertyName("total_bookmarks")]
        public long TotalBookmarks { get; set; }

        [JsonPropertyName("is_bookmarked")]
        public bool IsBookmarked { get; set; }

        [JsonPropertyName("visible")]
        public bool Visible { get; set; }

        [JsonPropertyName("is_muted")]
        public bool IsMuted { get; set; }

        [JsonPropertyName("illust_ai_type")]
        public long IllustAiType { get; set; }

        [JsonPropertyName("illust_book_style")]
        public long IllustBookStyle { get; set; }
    }
}
