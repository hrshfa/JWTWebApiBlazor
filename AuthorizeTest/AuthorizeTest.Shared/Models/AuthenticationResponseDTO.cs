namespace AuthorizeTest.Shared.Models
{
    public class AuthenticationResponseDTO
    {
        public bool IsAuthSuccessful { get; set; }
        public string ErrorMessage { get; set; }
        public JWTTokenDTO JWTToken { get; set; }
        public UserDTO UserDTO { get; set; }
    }
}
