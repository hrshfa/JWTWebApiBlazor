using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AuthorizeTest.Shared.Models
{
    public class JWTTokenDTO
    {
        public string AccessToken { get; set; }
        [JsonPropertyName("refreshToken")]
        [Required]
        public string RefreshToken { get; set; }
        public int AccessTokenExpirationMinutes { get; set; } = 0;
        public int RefreshTokenExpirationMinutes { get; set; } = 0;
        [JsonIgnore]
        public string RefreshTokenSerial { get; set; }
        [JsonIgnore]
        public IEnumerable<Claim> Claims { get; set; }
    }
}
