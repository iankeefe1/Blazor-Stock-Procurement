namespace StockApp.Shared.Dtos.Query
{
    /// <summary>Satu syarat filter untuk satu kolom. Value selalu teks: angka "12.5", tanggal "2026-11-28", bool "true".</summary>
    public class FilterCriterion
    {
        public string Field { get; set; } = "";
        public FilterOp Op { get; set; }
        public string Value { get; set; } = "";
    }
}