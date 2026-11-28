namespace StockApp.Shared.Dtos.Query
{
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = [];
        public int TotalCount { get; set; }          // jumlah semua baris yang cocok filter
    }
}