namespace SoraBot.Dto.Lolicon
{
    internal class LoliconImage
    {
        public int PID { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int UID { get; set; }
        public string Author { get; set; } = string.Empty;
        public LoliconImageUrls Urls { get; set; } = new();
    }
}
