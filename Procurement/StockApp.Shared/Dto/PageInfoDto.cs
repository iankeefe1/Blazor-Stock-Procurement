namespace StockApp.Shared.Dtos
{
    public class PageInfoDto
    {
        public string Key { get; set; } = "";          // mis. "dashboard", "master/barang"
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string Icon { get; set; } = "";         // kunci ikon, mis. "dashboard"
    }
}