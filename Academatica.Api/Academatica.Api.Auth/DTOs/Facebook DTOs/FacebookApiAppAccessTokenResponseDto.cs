using Newtonsoft.Json;

namespace SmartMath.Api.Auth.DTOs
{
    internal class FacebookApiAppAccessTokenResponseDto
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
        [JsonProperty("token_type")]
        public string TokenType { get; set; }
    }
}
