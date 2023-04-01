using System.Text.Json.Serialization;

namespace SoraBot.Dto
{
    internal class AuthResponse
    {
        [JsonPropertyName("has_error")]
        public bool HasError { get; set; } = false;

        [JsonPropertyName("error")]
        public string Error { get; set; } = string.Empty;

        [JsonPropertyName("error_description")]
        public string ErrorDescription { get; set; } = string.Empty;

        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;

        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
