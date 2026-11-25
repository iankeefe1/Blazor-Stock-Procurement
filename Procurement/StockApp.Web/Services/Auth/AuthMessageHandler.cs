using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;

namespace StockApp.Web.Services.Auth
{
    // Menempelkan token ke setiap request dan mengeluarkan user kalau token ditolak (401).
    public class AuthMessageHandler(TokenStore store, ApiAuthStateProvider provider, NavigationManager nav)
        : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await store.GetAsync();
            if (!string.IsNullOrEmpty(token))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await base.SendAsync(request, cancellationToken);

            var isLogin = request.RequestUri?.AbsolutePath.EndsWith("/auth/login", StringComparison.OrdinalIgnoreCase) == true;
            if (response.StatusCode == HttpStatusCode.Unauthorized && !isLogin)
            {
                await store.ClearAsync();
                provider.NotifyChanged();
                nav.NavigateTo("/login");
            }

            return response;
        }
    }
}