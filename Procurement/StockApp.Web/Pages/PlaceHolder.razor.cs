using Microsoft.AspNetCore.Components;
using MudBlazor;


namespace StockApp.Web.Pages
{
    public partial class PlaceHolder
    {
        [Inject] private NavigationManager Nav { get; set; } = default!;

        private static readonly Dictionary<string, string> Titles = new()
        {
            ["master/barang"] = "Master Barang",
            ["master/supplier"] = "Master Supplier",
            ["master/gudang"] = "Master Gudang",
            ["master/kategori"] = "Kategori & Satuan",
            ["transaksi/po"] = "Purchase Order",
            ["transaksi/penerimaan"] = "Penerimaan Barang",
            ["transaksi/pengeluaran"] = "Pengeluaran Barang",
            ["transaksi/opname"] = "Stock Opname",
            ["laporan/stok"] = "Stok saat ini",
            ["laporan/kartu-stok"] = "Kartu stok",
            ["laporan/pergerakan"] = "Pergerakan stok",
            ["admin/user"] = "User & Role",
            ["admin/log-aktivitas"] = "Activity log",
            ["admin/log-login"] = "Login log",
            ["admin/log-error"] = "Error log",
        };
        private string Title =>
            Titles.TryGetValue(Nav.ToBaseRelativePath(Nav.Uri).TrimEnd('/'), out var t) ? t : "Halaman";
    }
}