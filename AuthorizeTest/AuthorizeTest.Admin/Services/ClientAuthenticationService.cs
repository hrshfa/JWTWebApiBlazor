using AuthorizeTest.Admin.Models;
using AuthorizeTest.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AuthorizeTest.Admin.Services
{
    public interface IClientAuthenticationService
    {
        public Task<AuthenticationResponseDTO> LoginAsync(AuthenticationDTO userFromAuthentication);
        public Task LogoutAsync();
        public Task<RegistrationResponseDTO> RegisterUserAsync(UserRequestDTO userForRegisteration);
    }
    public class ClientAuthenticationService : IClientAuthenticationService
    {
        private readonly HttpService _http;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;

        public ClientAuthenticationService(HttpService http, ILocalStorageService localStorage,AuthenticationStateProvider authStateProvider)
        {
            _http = http;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;
        }

        public async Task<AuthenticationResponseDTO> LoginAsync(AuthenticationDTO userFromAuthentication)
        {

            var result = await _http.Post<AuthenticationResponseDTO>("account/login", userFromAuthentication,false);

            if (result is not null)
            {
                await _localStorage.SetItemAsync(ConstantKeys.LocalJWTToken, result.JWTToken);
                await _localStorage.SetItemAsync(ConstantKeys.LocalUserDetails, result.UserDTO);
                await _localStorage.SetItemAsync(ConstantKeys.LocalSavedTokenDateTime, DateTime.UtcNow);
                ((AuthStateProvider)_authStateProvider).NotifyUserLoggedIn(result.JWTToken.AccessToken);
                return new AuthenticationResponseDTO { IsAuthSuccessful = true };
            }
            else
            {
                return new AuthenticationResponseDTO { IsAuthSuccessful = false,ErrorMessage="ورود ناموفق" }; ;
            }
        }
        public async Task LogoutAsync()
        {
            var token = await _localStorage.GetItemAsync<JWTTokenDTO>(ConstantKeys.LocalJWTToken);
            var result = await _http.Get<bool>($"account/Logout?refreshToken={token.RefreshToken}");
            if (result)
            {
                await _localStorage.RemoveItemAsync(ConstantKeys.LocalJWTToken);
                await _localStorage.RemoveItemAsync(ConstantKeys.LocalUserDetails);
                await _localStorage.RemoveItemAsync(ConstantKeys.LocalSavedTokenDateTime);
                ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
            }
            else
            {
                throw new Exception("Error Logout");
            }
        }

        public async Task<RegistrationResponseDTO> RegisterUserAsync(UserRequestDTO userForRegisteration)
        {
            var result = await _http.Post<RegistrationResponseDTO>("account/signup", userForRegisteration);

            if (result is not null)
            {
                return new RegistrationResponseDTO { IsRegistrationSuccessful = true };
            }
            else
            {
                return result;
            }
        }
    }
}
