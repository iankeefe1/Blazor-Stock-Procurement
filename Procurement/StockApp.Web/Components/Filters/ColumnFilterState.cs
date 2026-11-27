using System.Globalization;

namespace StockApp.Web.Components.Filters
{
    public enum ColumnFilterType { Text, Number, Date, Bool }

    public enum ColumnFilterOp
    {
        Contains, NotContains, Equal, NotEqual,
        LessThan, GreaterThan, NotLessThan, NotGreaterThan,
    }

    /// <summary>Isi filter satu kolom. Dipakai oleh ColumnFilter dan oleh halaman untuk menyaring data.</summary>
    public class ColumnFilterState(ColumnFilterType type)
    {
        public ColumnFilterType Type { get; } = type;
        public ColumnFilterOp Operator { get; set; } = DefaultOp(type);
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

        // ---- pencocokan data ----

        public bool Matches(string? value)
        {
            if (!IsActive) return true;
            var v = (value ?? "").Trim();
            var t = Text!.Trim();
            return Operator switch
            {
                ColumnFilterOp.Contains => v.Contains(t, StringComparison.OrdinalIgnoreCase),
                ColumnFilterOp.NotContains => !v.Contains(t, StringComparison.OrdinalIgnoreCase),
                ColumnFilterOp.Equal => string.Equals(v, t, StringComparison.OrdinalIgnoreCase),
                ColumnFilterOp.NotEqual => !string.Equals(v, t, StringComparison.OrdinalIgnoreCase),
                _ => true,
            };
        }

        public bool Matches(decimal value)
        {
            if (!IsActive) return true;
            var n = Number!.Value;
            return Compare(value.CompareTo(n));
        }

        public bool Matches(DateTime value)
        {
            if (!IsActive) return true;
            return Compare(value.Date.CompareTo(Date!.Value.Date));
        }

        public bool Matches(bool value) => !IsActive || Bool == value;

        // cmp: hasil CompareTo (nilai data dibanding nilai filter)
        private bool Compare(int cmp) => Operator switch
        {
            ColumnFilterOp.Equal => cmp == 0,
            ColumnFilterOp.NotEqual => cmp != 0,
            ColumnFilterOp.LessThan => cmp < 0,
            ColumnFilterOp.GreaterThan => cmp > 0,
            ColumnFilterOp.NotLessThan => cmp >= 0,
            ColumnFilterOp.NotGreaterThan => cmp <= 0,
            _ => true,
        };

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

        public static ColumnFilterOp DefaultOp(ColumnFilterType type) =>
            type == ColumnFilterType.Text ? ColumnFilterOp.Contains : ColumnFilterOp.Equal;

        public static IReadOnlyList<ColumnFilterOp> OperatorsFor(ColumnFilterType type) => type switch
        {
            ColumnFilterType.Text => [ColumnFilterOp.Contains, ColumnFilterOp.Equal,
                                      ColumnFilterOp.NotContains, ColumnFilterOp.NotEqual],
            ColumnFilterType.Bool => [ColumnFilterOp.Equal],
            _ => [ColumnFilterOp.Equal, ColumnFilterOp.NotEqual,
                  ColumnFilterOp.LessThan, ColumnFilterOp.GreaterThan,
                  ColumnFilterOp.NotLessThan, ColumnFilterOp.NotGreaterThan],
        };

        public static string LabelFor(ColumnFilterOp op, ColumnFilterType type)
        {
            var date = type == ColumnFilterType.Date;
            return op switch
            {
                ColumnFilterOp.Contains => "Mengandung",
                ColumnFilterOp.NotContains => "Tidak mengandung",
                ColumnFilterOp.Equal => "Sama dengan",
                ColumnFilterOp.NotEqual => "Tidak sama dengan",
                ColumnFilterOp.LessThan => date ? "Sebelum" : "Kurang dari",
                ColumnFilterOp.GreaterThan => date ? "Setelah" : "Lebih dari",
                ColumnFilterOp.NotLessThan => date ? "Tidak sebelum" : "Tidak kurang dari",
                ColumnFilterOp.NotGreaterThan => date ? "Tidak setelah" : "Tidak lebih dari",
                _ => op.ToString(),
            };
        }
    }
}