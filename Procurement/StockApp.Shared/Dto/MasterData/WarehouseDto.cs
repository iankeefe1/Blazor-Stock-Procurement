namespace StockApp.Shared.Dtos
{
    public class WarehouseDto
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string Name { get; set; } = "";
        public string? Address { get; set; }
        public bool IsActive { get; set; }
    }
}