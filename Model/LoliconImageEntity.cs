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
        public string Title { get; set; }
        public List<string> Tags { get; set; }
        public int UID { get; set; }
        public string Author { get; set; }
        public LoliconImageUrlsEntity Urls { get; set; }
    }
}
