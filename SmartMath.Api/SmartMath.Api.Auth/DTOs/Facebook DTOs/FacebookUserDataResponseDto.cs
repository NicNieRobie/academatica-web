using Newtonsoft.Json;
using SmartMath.Api.Auth.Models;

namespace SmartMath.Api.Auth.DTOs
{
    public class FacebookUserDataResponseDto
    {
        public long Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        [JsonProperty("first_name")]
        public string FirstName { get; set; }
        [JsonProperty("last_name")]
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Locale { get; set; }
        public FacebookPicture Picture { get; set; }
    }
}
