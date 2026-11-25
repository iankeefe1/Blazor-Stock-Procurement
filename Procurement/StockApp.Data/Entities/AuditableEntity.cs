namespace StockApp.Data.Entities
{
    public abstract class AuditableEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;  
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}