namespace StockApp.Data.Entities
{
    public class LoginLog
    {
        public long Id { get; set; }
        public string UserName { get; set; } = "";   // yang diketik user, termasuk saat gagal
        public string? UserId { get; set; }          // null kalau username tidak ada
        public bool Success { get; set; }
        public string? FailReason { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}