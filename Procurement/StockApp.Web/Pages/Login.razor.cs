using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using StockApp.Web.Services.Auth;

namespace StockApp.Web.Pages
{
    public partial class Login
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuthService Auth { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;

        private string username = "";
        private string password = "";
        private string? error;
        private bool showPassword;
        private bool loading;

        // Kalau sudah login, tidak perlu melihat halaman login lagi.
        protected override async Task OnInitializedAsync()
        {
            var user = (await AuthProvider.GetAuthenticationStateAsync()).User;
            if (user.Identity?.IsAuthenticated == true)
                Nav.NavigateTo("/dashboard");
        }

        private void TogglePassword() => showPassword = !showPassword;

        private async Task OnPasswordKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                await SignIn();
        }

        private async Task SignIn()
        {
            if (loading) return;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                error = "Username dan password wajib diisi.";
                return;
            }

            error = null;
            loading = true;
            error = await Auth.LoginAsync(username.Trim(), password);   // null = berhasil
            loading = false;

            if (error is null)
                Nav.NavigateTo("/dashboard");
        }
    }
}