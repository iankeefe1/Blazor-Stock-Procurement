using System.Globalization;
using StockApp.Shared.Dtos.Query;

namespace StockApp.Web.Components.Filters
{
    public enum ColumnFilterType { Text, Number, Date, Bool }

    /// <summary>Isi filter satu kolom di halaman. Diubah menjadi FilterCriterion untuk dikirim ke API.</summary>
    public class ColumnFilterState(ColumnFilterType type)
    {
        public ColumnFilterType Type { get; } = type;
        public FilterOp Operator { get; set; } = DefaultOp(type);
        public string? Text { get; set; }
        public decimal? Number { get; set; }
        public DateTime? Date { get; set; }
        public bool? Bool { get; set; }

        public bool IsActive => Type switch
        {
            ColumnFilterType.Text => !string.IsNullOrWhiteSpace(Text),
            ColumnFilterType.Number => Number.HasValue,
            ColumnFilterType.Date => Date.HasValue,
            _ => Bool.HasValue,
        };

        public void Clear()
        {
            Operator = DefaultOp(Type);
            Text = null; Number = null; Date = null; Bool = null;
        }

        public void CopyFrom(ColumnFilterState other)
        {
            Operator = other.Operator;
            Text = other.Text; Number = other.Number; Date = other.Date; Bool = other.Bool;
        }

        /// <summary>Filter untuk dikirim ke API, atau null kalau filter belum diisi.</summary>
        public FilterCriterion? ToCriterion(string field)
        {
            if (!IsActive) return null;

            var value = Type switch
            {
                ColumnFilterType.Text => Text!.Trim(),
                ColumnFilterType.Number => Number!.Value.ToString(CultureInfo.InvariantCulture),
                ColumnFilterType.Date => Date!.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                _ => Bool == true ? "true" : "false",
            };
            return new FilterCriterion { Field = field, Op = Operator, Value = value };
        }

        // ---- teks untuk tampilan ----

        public string Describe(string title, string trueLabel = "Ya", string falseLabel = "Tidak")
        {
            if (Type == ColumnFilterType.Bool)
                return $"{title}: {(Bool == true ? trueLabel : falseLabel)}";

            var value = Type switch
            {
                ColumnFilterType.Text => $"\"{Text!.Trim()}\"",
                ColumnFilterType.Number => Number!.Value.ToString("0.##", CultureInfo.CurrentCulture),
                _ => Date!.Value.ToString("dd MMM yyyy", CultureInfo.CurrentCulture),
            };
            return $"{title} {LabelFor(Operator, Type).ToLowerInvariant()} {value}";
        }

        public static FilterOp DefaultOp(ColumnFilterType type) =>
            type == ColumnFilterType.Text ? FilterOp.Contains : FilterOp.Equal;

        public static IReadOnlyList<FilterOp> OperatorsFor(ColumnFilterType type) => type switch
        {
            ColumnFilterType.Text => [FilterOp.Contains, FilterOp.Equal, FilterOp.NotContains, FilterOp.NotEqual],
            ColumnFilterType.Bool => [FilterOp.Equal],
            _ => [FilterOp.Equal, FilterOp.NotEqual, FilterOp.LessThan, FilterOp.GreaterThan,
                  FilterOp.NotLessThan, FilterOp.NotGreaterThan],
        };

        public static string LabelFor(FilterOp op, ColumnFilterType type)
        {
            var date = type == ColumnFilterType.Date;
            return op switch
            {
                FilterOp.Contains => "Mengandung",
                FilterOp.NotContains => "Tidak mengandung",
                FilterOp.Equal => "Sama dengan",
                FilterOp.NotEqual => "Tidak sama dengan",
                FilterOp.LessThan => date ? "Sebelum" : "Kurang dari",
                FilterOp.GreaterThan => date ? "Setelah" : "Lebih dari",
                FilterOp.NotLessThan => date ? "Tidak sebelum" : "Tidak kurang dari",
                FilterOp.NotGreaterThan => date ? "Tidak setelah" : "Tidak lebih dari",
                _ => op.ToString(),
            };
        }
    }
}