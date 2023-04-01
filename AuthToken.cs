using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SoraBot
{
    internal class AuthToken
    {
        public string AccessToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
