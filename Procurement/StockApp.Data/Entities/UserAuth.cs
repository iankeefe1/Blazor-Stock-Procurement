namespace StockApp.Data.Entities
{
    public class UserAuth : AuditableEntity
    {
        public int EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        public string UserName { get; set; } = "";
        public string PasswordHash { get; set; } = "";

        public int FailedCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }
}