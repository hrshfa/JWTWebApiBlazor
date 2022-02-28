using AuthorizeTest.Admin.Models;
using AuthorizeTest.Admin.Utils;
using AuthorizeTest.Shared.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;

namespace AuthorizeTest.Admin.Services
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _client;
        private readonly ILocalStorageService _localStorage;

        public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<JWTTokenDTO>(ConstantKeys.LocalJWTToken);
            var LocalSavedTokenDateTime = await _localStorage.GetItemAsync<DateTime>(ConstantKeys.LocalSavedTokenDateTime);
            if (token == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            var jwtInfo = JwtParser.ParseClaimsFromJwt(token.AccessToken);
            if (jwtInfo == null)
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
            if (LocalSavedTokenDateTime.AddMinutes(token.AccessTokenExpirationMinutes) < DateTime.UtcNow)
            {
                if (LocalSavedTokenDateTime.AddMinutes(token.RefreshTokenExpirationMinutes) > DateTime.UtcNow)
                {
                    var response = await _client.PostAsJsonAsync($"account/refreshToken", token);
                    var result = await response.Content.ReadFromJsonAsync<AuthenticationResponseDTO>();
                    if (response.IsSuccessStatusCode)
                    {
                        await _localStorage.SetItemAsync(ConstantKeys.LocalJWTToken, result.JWTToken);
                        await _localStorage.SetItemAsync(ConstantKeys.LocalSavedTokenDateTime, DateTime.UtcNow);
                        jwtInfo = JwtParser.ParseClaimsFromJwt(result.JWTToken.AccessToken);
                        return new AuthenticationState(
                            new ClaimsPrincipal(
                                new ClaimsIdentity(jwtInfo.Claims, "jwtAuthType")
                            )
                        );
                    }
                    else
                    {
                        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                    }
                }
                else
                {
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

            }
            
            return new AuthenticationState(
                        new ClaimsPrincipal(
                            new ClaimsIdentity(jwtInfo.Claims, "jwtAuthType")
                        )
                    );
        }
        public void NotifyUserLoggedIn(string token)
        {
            var jwtInfo = JwtParser.ParseClaimsFromJwt(token);
            //if (jwtInfo?.IsExpired == false)
            {
                var authenticatedUser = new ClaimsPrincipal(
                                       new ClaimsIdentity(jwtInfo.Claims, "jwtAuthType")
                                   );
                var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
                base.NotifyAuthenticationStateChanged(authState);
            }
        }

        public async void NotifyUserLogout()
        {
            await _localStorage.RemoveItemAsync(ConstantKeys.LocalJWTToken);
            await _localStorage.RemoveItemAsync(ConstantKeys.LocalUserDetails);
            await _localStorage.RemoveItemAsync(ConstantKeys.LocalSavedTokenDateTime);
            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity())));
            base.NotifyAuthenticationStateChanged(authState);
        }
    }
}
