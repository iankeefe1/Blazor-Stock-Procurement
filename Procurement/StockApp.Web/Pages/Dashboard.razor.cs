using MudBlazor;

namespace StockApp.Web.Pages
{
    public partial class Dashboard
    {
        private record Stat(string Label, string Value, string Note, string Icon, Color Color);
        private record LowStockRow(string Name, int Stock, int Min);
        private record CategoryRow(string Name, int Qty, Color Color);
        private record ActivityRow(string Text, string When, string Icon, Color Color);
        private readonly Stat[] stats =
        {
            new("Jumlah barang", "248", "di 3 gudang", Icons.Material.Filled.Inventory2, Color.Primary),
            new("Stok menipis", "12", "di bawah stok minimum", Icons.Material.Filled.Warning, Color.Warning),
            new("Masuk hari ini", "3", "dokumen penerimaan", Icons.Material.Filled.MoveToInbox, Color.Success),
            new("Keluar hari ini", "5", "dokumen pengeluaran", Icons.Material.Filled.Outbox, Color.Info),
        };

        private readonly LowStockRow[] lowStock =
        {
            new("Kertas A4 80gsm", 4, 20),
            new("Tinta printer hitam", 0, 5),
            new("Map plastik", 11, 30),
            new("Spidol whiteboard", 3, 12),
        };

        private readonly CategoryRow[] categories =
        {
            new("ATK", 420, Color.Primary),
            new("Elektronik", 130, Color.Secondary),
            new("Kebersihan", 95, Color.Tertiary),
        };

        private double maxCategory => categories.Max(c => c.Qty);

        private readonly ActivityRow[] activities =
        {
            new("Budi menerima GR-2026-0012", "10:42", Icons.Material.Filled.MoveToInbox, Color.Success),
            new("Sari membuat PO-2026-0031", "10:15", Icons.Material.Filled.Description, Color.Primary),
            new("Andi mengeluarkan GI-2026-0044", "09:58", Icons.Material.Filled.Outbox, Color.Info),
            new("Admin menyetujui opname SO-2026-0003", "09:20", Icons.Material.Filled.FactCheck, Color.Secondary),
        };
    }
}