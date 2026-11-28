namespace StockApp.Shared.Dtos.Query
{
    public class PagedRequest
    {
        public int Page { get; set; } = 1;          // mulai dari 1
        public int PageSize { get; set; } = 10;     // maksimal 100 (dijaga API)
        public List<FilterCriterion> Filters { get; set; } = [];
    }
}