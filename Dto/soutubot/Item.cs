using Newtonsoft.Json;
using SauceNET.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperHime.Dto.soutubot
{
    internal class Item
    {
        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("page")]
        public long Page { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("pagePath")]
        public string PagePath { get; set; }

        [JsonProperty("subjectPath")]
        public string SubjectPath { get; set; }

        [JsonProperty("previewImageUrl")]
        public Uri PreviewImageUrl { get; set; }

        [JsonProperty("similarity")]
        public double Similarity { get; set; }

        
    }
}
