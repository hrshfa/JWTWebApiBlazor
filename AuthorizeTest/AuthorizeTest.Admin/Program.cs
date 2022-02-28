using AuthorizeTest.Admin;
using AuthorizeTest.Admin.Services;
using AuthorizeTest.Shared.Utils;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddAuthorizationCore(options => options.AddAppPolicies());

builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthStateProvider>();
builder.Services.AddScoped<IClientAuthenticationService, ClientAuthenticationService>();
builder.Services.AddScoped<HttpService>();


builder.Services.AddScoped(x =>
{

    Uri apiUrl;
    if (builder.HostEnvironment.IsDevelopment())
    {
        apiUrl = new Uri(builder.Configuration["localAPIUrl"]);
    }
    else
    {
        apiUrl = new Uri(builder.Configuration["siteAPIUrl"]);
    }

    return new HttpClient() { BaseAddress = apiUrl };
});




//builder.Services.AddHttpClient(
//                    name: "ServerAPI",
//                    configureClient: client =>
//                    {
//                        client.BaseAddress = new Uri(builder.Configuration.GetValue<string>("BaseAPIUrl"));
//                        client.DefaultRequestHeaders.Add("User-Agent", "BlazorWasm.Client 1.0");
//                    }
//                )
//                .AddHttpMessageHandler<ClientHttpInterceptorService>();
//builder.Services.AddScoped<ClientHttpInterceptorService>();
//builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI"));

await builder.Build().RunAsync();
