using StockApp.Shared.Dtos;

namespace StockApp.Web.Services
{
    // SEMENTARA: data contoh di memori. Nanti diganti ApiPageInfoService yang membaca dari database.
    public class InMemoryPageInfoService : IPageInfoService
    {
        private static readonly Dictionary<string, PageInfoDto> Pages = new[]
        {
            P("dashboard", "Dashboard", "Ringkasan stok dan aktivitas terbaru: barang menipis serta dokumen masuk dan keluar hari ini.", "dashboard"),
            P("master/barang", "Master Barang", "Kelola daftar barang yang stoknya dipantau: kode, kategori, satuan, dan stok minimum.", "inventory"),
            P("master/supplier", "Master Supplier", "Kelola data pemasok: kontak, alamat, dan status aktif.", "truck"),
            P("master/gudang", "Master Gudang", "Kelola lokasi penyimpanan barang.", "warehouse"),
            P("master/kategori", "Kategori & Satuan", "Kelola kategori barang dan satuan ukur.", "category"),
            P("transaksi/po", "Purchase Order", "Buat dan pantau pesanan pembelian ke supplier beserta statusnya.", "po"),
            P("transaksi/penerimaan", "Penerimaan Barang", "Catat barang masuk berdasarkan PO; stok bertambah otomatis.", "inbox"),
            P("transaksi/pengeluaran", "Pengeluaran Barang", "Catat barang keluar; sistem menolak jumlah yang melebihi stok.", "outbox"),
            P("transaksi/opname", "Stock Opname", "Cocokkan stok fisik dengan stok sistem dan ajukan penyesuaian.", "opname"),
            P("laporan/stok", "Stok saat ini", "Posisi stok per barang dan gudang beserta status stok minimum.", "chart"),
            P("laporan/kartu-stok", "Kartu stok", "Riwayat mutasi per barang: masuk, keluar, dan saldo.", "book"),
            P("laporan/pergerakan", "Pergerakan stok", "Ringkasan saldo awal, masuk, keluar, dan saldo akhir per periode.", "swap"),
            P("admin/user", "User & Role", "Kelola akun pengguna dan hak aksesnya.", "users"),
            P("admin/log-aktivitas", "Activity log", "Riwayat aksi pengguna: siapa mengubah apa dan kapan.", "history"),
            P("admin/log-login", "Login log", "Riwayat login berhasil dan gagal beserta alamat IP.", "key"),
            P("admin/log-error", "Error log", "Catatan error aplikasi untuk pemantauan dan perbaikan.", "bug"),
        }.ToDictionary(p => p.Key);

        private static PageInfoDto P(string key, string title, string description, string icon) =>
            new() { Key = key, Title = title, Description = description, Icon = icon };

        public Task<PageInfoDto?> GetAsync(string key) =>
            Task.FromResult(Pages.TryGetValue(key, out var info) ? info : null);
    }
}