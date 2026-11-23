using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace StockApp.Web.Layout
{
    public partial class MainLayout : IDisposable
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private bool _isDarkMode;
        private bool _menuOpen = true;
        private bool _profileOpen;

        private record UserInfo(string Name, string Role, string Username, string Email,
                                string Warehouse, string LastLogin)
        {
            public string Initials =>
                string.Concat(Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpper(p[0])));
        }

        private record MenuItemInfo(string Label, string Href);
        private record MenuGroup(string Title, string Icon, string Prefix, MenuItemInfo[] Items);

        // SEMENTARA: data contoh. Nanti dari login/API.
        private UserInfo CurrentUser { get; } = new(
            "Ian Keefe", "Administrator", "ian", "ian@perusahaan.co.id", "Gudang Utama", "Hari ini, 08:12");

        // Tambah halaman baru cukup menambah satu baris di sini.
        private static readonly MenuGroup[] Groups =
        {
            new("Master Data", Icons.Material.Filled.Storage, "master", new MenuItemInfo[]
            {
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
                new("User & Role", "/admin/user"),
                new("Activity log", "/admin/log-aktivitas"),
                new("Login log", "/admin/log-login"),
                new("Error log", "/admin/log-error"),
            }),
        };

        private string CurrentPath => Nav.ToBaseRelativePath(Nav.Uri);
        private bool IsHome => CurrentPath.StartsWith("dashboard", StringComparison.OrdinalIgnoreCase);
        private bool IsActive(string prefix) => CurrentPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

        private void SignOut() => Nav.NavigateTo("/login");

        protected override void OnInitialized() => Nav.LocationChanged += OnLocationChanged;

        private void OnLocationChanged(object? sender, LocationChangedEventArgs e) => StateHasChanged();

        public void Dispose() => Nav.LocationChanged -= OnLocationChanged;
    }
}