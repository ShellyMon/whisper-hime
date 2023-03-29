using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoraBot.Model
{
    internal class LoliconImageEntity
    {
        public int PID { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public int UID { get; set; }
        public string Author { get; set; } = string.Empty;
        public LoliconImageUrlsEntity Urls { get; set; } = new();
    }
}
