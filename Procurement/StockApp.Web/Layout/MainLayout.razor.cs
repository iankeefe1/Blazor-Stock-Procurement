using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;
using StockApp.Web.Services.Auth;

namespace StockApp.Web.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;
        [Inject] private AuthenticationStateProvider AuthProvider { get; set; } = default!;
        [Inject] private AuthService Auth { get; set; } = default!;

        private bool _isDarkMode;
        private bool _menuOpen = true;
        private bool _profileOpen;

        private record UserInfo(string Name, string Role, string Username)
        {
            public string Initials =>
                string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));
        }

        private record MenuItemInfo(string Label, string Href);
        private record MenuGroup(string Title, string Icon, string Prefix, MenuItemInfo[] Items);

        private UserInfo CurrentUser { get; set; } = new("", "", "");

        // Tambah halaman baru cukup menambah satu baris di sini.
        private static readonly MenuGroup[] Groups =
        {
            new("Master Data", Icons.Material.Filled.Storage, "master", new MenuItemInfo[]
            {
                new("Employee", "/master/employee"),
                new("Master Barang", "/master/barang"),
                new("Master Supplier", "/master/supplier"),
                new("Master Gudang", "/master/gudang"),
                new("Kategori & Satuan", "/master/kategori"),
            }),
            new("Transaksi", Icons.Material.Filled.SwapHoriz, "transaksi", new MenuItemInfo[]
            {
                new("Purchase Order", "/transaksi/po"),
                new("Penerimaan Barang", "/transaksi/penerimaan"),
                new("Pengeluaran Barang", "/transaksi/pengeluaran"),
                new("Stock Opname", "/transaksi/opname"),
            }),
            new("Laporan", Icons.Material.Filled.Assessment, "laporan", new MenuItemInfo[]
            {
                new("Stok saat ini", "/laporan/stok"),
                new("Kartu stok", "/laporan/kartu-stok"),
                new("Pergerakan stok", "/laporan/pergerakan"),
            }),
            new("Administrasi", Icons.Material.Filled.AdminPanelSettings, "admin", new MenuItemInfo[]
            {
                new("Activity log", "/admin/log-aktivitas"),
                new("Login log", "/admin/log-login"),
                new("Error log", "/admin/log-error"),
            }),
        };

        private string CurrentPath => Nav.ToBaseRelativePath(Nav.Uri);
        private bool IsActive(string prefix) => CurrentPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        protected override async Task OnInitializedAsync()
        {
            Nav.LocationChanged += OnLocationChanged;

            var user = (await AuthProvider.GetAuthenticationStateAsync()).User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var userName = user.Identity.Name ?? "";
                CurrentUser = new UserInfo(
                    user.FindFirst("full_name")?.Value ?? userName,
                    string.Join(", ", user.FindAll("role").Select(c => c.Value)),
                    userName);
            }
        }

        private async Task SignOutAsync()
        {
            await Auth.LogoutAsync();
            Nav.NavigateTo("/login");
        }

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => StateHasChanged();

        public void Dispose() => Nav.LocationChanged -= OnLocationChanged;
    }
}