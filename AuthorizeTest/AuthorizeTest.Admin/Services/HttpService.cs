using AuthorizeTest.Admin.Utils;
using AuthorizeTest.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace AuthorizeTest.Admin.Services
{
    public class HttpService
    {

        private HttpClient _httpClient;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private readonly AuthenticationStateProvider _authStateProvider;
        public Uri BaseAddress { get; private set; }

        public HttpService(
            HttpClient httpClient,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService, AuthenticationStateProvider authStateProvider
        )
        {
            _httpClient = httpClient;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
            BaseAddress = _httpClient.BaseAddress;
            _authStateProvider = authStateProvider;
        }

        public async Task<T> Get<T>(string uri, bool isReturnToLogin = true)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Get, uri);
                return await sendRequest<T>(request, isReturnToLogin);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<T> Post<T>(string uri, object value, bool isReturnToLogin = true)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");
                return await sendRequest<T>(request, isReturnToLogin);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

        }

        public async Task<T> PostFile<T>(MultipartFormDataContent content, string uri)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = content;
                return await sendRequest<T>(request);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        // helper methods
        private async Task<T> sendRequest<T>(HttpRequestMessage request, bool isReturnToLogin = true)
        {
            try
            {
                // How to add a JWT to all of the requests
                var token = await _localStorageService.GetItemAsync<JWTTokenDTO>(ConstantKeys.LocalJWTToken);
                var LocalSavedTokenDateTime = await _localStorageService.GetItemAsync<DateTime>(ConstantKeys.LocalSavedTokenDateTime);

                if (token is not null)
                {
                    if (LocalSavedTokenDateTime.AddMinutes(token.AccessTokenExpirationMinutes) < DateTime.UtcNow)// access token is expired
                    {
                        if (LocalSavedTokenDateTime.AddMinutes(token.RefreshTokenExpirationMinutes) > DateTime.UtcNow) // refresh token do not expire
                        {
                            var refreshTokenMessage = new HttpRequestMessage
                            {
                                Method = new HttpMethod("GET"),
                                RequestUri = new Uri("account/refreshToken")
                            };
                            var newTokenResponse = await _httpClient.SendAsync(refreshTokenMessage);
                            if (newTokenResponse.IsSuccessStatusCode)
                            {
                                var resultStream = await newTokenResponse.Content.ReadAsStreamAsync();
                                var result = await JsonSerializer.DeserializeAsync<AuthenticationResponseDTO>(resultStream);

                                await _localStorageService.SetItemAsync(ConstantKeys.LocalJWTToken, result.JWTToken);
                                await _localStorageService.SetItemAsync(ConstantKeys.LocalUserDetails, result.UserDTO);
                                await _localStorageService.SetItemAsync(ConstantKeys.LocalSavedTokenDateTime, DateTime.UtcNow);
                                ((AuthStateProvider)_authStateProvider).NotifyUserLoggedIn(result.JWTToken.AccessToken);
                                token = result.JWTToken;
                                request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
                            }
                            else// server unavailable or api method has changed or  Unauthorized
                            {
                                ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
                                request.Headers.Authorization = null;
                                if (newTokenResponse.StatusCode == HttpStatusCode.Unauthorized)
                                {
                                    if (isReturnToLogin)
                                    {
                                        var returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                                        if (string.IsNullOrEmpty(returnUrl))
                                        {
                                            _navigationManager.NavigateTo("login");
                                        }
                                        else
                                        {
                                            _navigationManager.NavigateTo($"login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                                        }
                                        return default;
                                    }
                                }
                                else if (isReturnToLogin) // server unavailable or api method has changed
                                {
                                    _navigationManager.NavigateTo("/404");
                                    return default;
                                }

                            }
                        }
                        else //refresh token is expired
                        {
                            ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
                            request.Headers.Authorization = null;
                            if (isReturnToLogin)
                            {
                                var returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                                if (string.IsNullOrEmpty(returnUrl))
                                {
                                    _navigationManager.NavigateTo("login");
                                }
                                else
                                {
                                    _navigationManager.NavigateTo($"login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                                }
                                return default;
                            }
                        }
                    }
                    else //access token is valid
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);

                    }
                }
                else // no any token is available
                {
                    ((AuthStateProvider)_authStateProvider).NotifyUserLogout();
                    request.Headers.Authorization = null;
                    if (isReturnToLogin)
                    {
                        var returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            _navigationManager.NavigateTo("login");
                        }
                        else
                        {
                            _navigationManager.NavigateTo($"login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                        }
                        return default;
                    }
                }

                var response = await _httpClient.SendAsync(request);

                // auto logout on 401 response
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    if (isReturnToLogin)
                    {
                        var returnUrl = _navigationManager.ToBaseRelativePath(_navigationManager.Uri);
                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            _navigationManager.NavigateTo("login");
                        }
                        else
                        {
                            _navigationManager.NavigateTo($"login?returnUrl={Uri.EscapeDataString(returnUrl)}");
                        }
                    }
                    return default;
                }

                if (!response.IsSuccessStatusCode)
                {
                    //   await _jsRuntime.ToastrError($"Failed to call `{request.RequestUri}`. StatusCode: {response.StatusCode}.");

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.NotFound:
                            _navigationManager.NavigateTo("/404");
                            break;
                        case HttpStatusCode.Forbidden: // 403
                        case HttpStatusCode.Unauthorized: // 401
                            _navigationManager.NavigateTo("/unauthorized");
                            break;
                        case HttpStatusCode.BadRequest:
                            var error = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                            throw new Exception(error["message"]);
                            break;
                        default:
                            _navigationManager.NavigateTo("/500");
                            break;
                    }
                    return default;
                }




                var responseContent = await response.Content.ReadFromJsonAsync<T>();
                return responseContent;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
