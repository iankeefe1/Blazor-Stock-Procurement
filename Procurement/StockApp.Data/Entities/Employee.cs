namespace StockApp.Data.Entities
{
    public class Employee : AuditableEntity
    {
        public string Nik { get; set; } = "";
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Position { get; set; } = "";
        public string Warehouse { get; set; } = "";   // sementara teks; nanti jadi relasi ke tabel Warehouses
        public string Role { get; set; } = "";        // sementara teks; nanti mengikuti role akun Identity
        public string? UserId { get; set; }           // diisi saat karyawan dibuatkan akun login
    }
}