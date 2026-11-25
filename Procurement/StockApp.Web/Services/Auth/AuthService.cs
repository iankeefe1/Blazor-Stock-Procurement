using System.Net.Http.Json;
using StockApp.Shared.Dtos.Auth;

namespace StockApp.Web.Services.Auth
{
    public class AuthService(HttpClient http, TokenStore store, ApiAuthStateProvider provider)
    {
        /// <summary>Mengembalikan pesan error, atau null jika login berhasil.</summary>
        public async Task<string?> LoginAsync(string username, string password)
        {
            try
            {
                var response = await http.PostAsJsonAsync("api/auth/login",
                    new LoginRequest { Username = username, Password = password });

                if (!response.IsSuccessStatusCode)
                    return await ApiError.ReadAsync(response);

                var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (body is null || string.IsNullOrEmpty(body.Token))
                    return "Respons server tidak valid.";

                await store.SetAsync(body.Token);
                provider.NotifyChanged();
                return null;
            }
            catch (HttpRequestException)
            {
                return "Tidak dapat terhubung ke server.";
            }
        }

        public async Task LogoutAsync()
        {
            await store.ClearAsync();
            provider.NotifyChanged();
        }
    }
}