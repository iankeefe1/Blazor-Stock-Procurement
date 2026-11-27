namespace StockApp.Data.Entities
{
    public class Warehouse : AuditableEntity
    {
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Address { get; set; }
    }
}