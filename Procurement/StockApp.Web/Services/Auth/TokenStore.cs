using Microsoft.JSInterop;

namespace StockApp.Web.Services.Auth
{
    // Menyimpan token login di localStorage browser.
    public class TokenStore(IJSRuntime js)
    {
        private const string Key = "stockapp.token";

        public ValueTask<string?> GetAsync() => js.InvokeAsync<string?>("localStorage.getItem", Key);
        public ValueTask SetAsync(string token) => js.InvokeVoidAsync("localStorage.setItem", Key, token);
        public ValueTask ClearAsync() => js.InvokeVoidAsync("localStorage.removeItem", Key);
    }
}