using System.Text.Json.Serialization;

namespace XcelerateLinks.Models.ViewModels
{
    public class AuthResponse
    {
        [JsonPropertyName("token")]
        public string Token { get; set; } = "";
        
        [JsonPropertyName("expiresAt")]
        public DateTime ExpiresAt { get; set; }
    }
}
