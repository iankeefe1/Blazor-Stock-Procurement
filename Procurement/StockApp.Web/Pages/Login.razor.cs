using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace StockApp.Web.Pages
{
    public partial class Login
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private string username = "";
        private string password = "";
        private string? error;
        private bool showPassword;

        private void TogglePassword() => showPassword = !showPassword;

        private void OnPasswordKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
                SignIn();
        }

        // SEMENTARA: belum ada autentikasi. Nanti diganti panggilan ke API
        // (ASP.NET Core Identity + JWT) yang juga mencatat Login log.
        private void SignIn()
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                error = "Username dan password wajib diisi.";
                return;
            }

            error = null;
            Nav.NavigateTo("/");
        }
    }
}
