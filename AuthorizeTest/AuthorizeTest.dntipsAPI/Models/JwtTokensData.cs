using System.Security.Claims;

namespace AuthorizeTest.dntipsAPI.Models
{
    public class JwtTokensData
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string RefreshTokenSerial { get; set; }
        public IEnumerable<Claim> Claims { get; set; }
        public int AccessTokenExpirationMinutes { get; set; } = 0;
        public int RefreshTokenExpirationMinutes { get; set; } = 0;
    }
}
