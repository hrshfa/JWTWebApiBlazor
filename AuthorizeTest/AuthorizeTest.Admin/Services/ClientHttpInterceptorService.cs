using AuthorizeTest.Admin.Models;
using AuthorizeTest.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

namespace AuthorizeTest.Admin.Services
{
    public class ClientHttpInterceptorService : DelegatingHandler
    {
        private readonly NavigationManager _navigationManager;
        private readonly ILocalStorageService _localStorage;
        private readonly IJSRuntime _jsRuntime;
        public ClientHttpInterceptorService(
                NavigationManager navigationManager,
                ILocalStorageService localStorage,
                IJSRuntime JsRuntime)
        {
            _navigationManager = navigationManager ?? throw new ArgumentNullException(nameof(navigationManager));
            _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
            _jsRuntime = JsRuntime ?? throw new ArgumentNullException(nameof(JsRuntime));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // How to add a JWT to all of the requests
                var token = await _localStorage.GetItemAsync<JWTTokenDTO>(ConstantKeys.LocalJWTToken);
                if (token is not null)
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("bearer", token.AccessToken);
                }
                else
                {
                    request.Headers.Authorization = null;
                }

                var response = await base.SendAsync(request, cancellationToken);

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
                        default:
                            _navigationManager.NavigateTo("/500");
                            break;
                    }
                    return default;
                }

                return response;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
    }
}
