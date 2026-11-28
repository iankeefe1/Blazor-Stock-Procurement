using System.Globalization;
using System.Linq.Expressions;
using System.Reflection;
using StockApp.Shared.Dtos.Query;

namespace StockApp.Api.Querying
{
    /// <summary>Filter tidak valid (kolom tidak dikenal, nilai salah, kondisi tidak berlaku). Dijadikan HTTP 400.</summary>
    public class FilterException(string message) : Exception(message);

    internal sealed record FilterBox<TV>(TV Value);

    /// <summary>Satu kolom yang boleh difilter. Dibuat lewat FilterFields.</summary>
    public abstract class FilterField<T>
    {
        public abstract Expression<Func<T, bool>> Build(FilterOp op, string raw);

        // Nilai dibungkus supaya EF menjadikannya parameter SQL (bukan teks mentah).
        protected static Expression Param<TV>(TV value) =>
            Expression.Property(Expression.Constant(new FilterBox<TV>(value)), nameof(FilterBox<TV>.Value));

        protected static FilterException BadOp(FilterOp op) =>
            new($"Kondisi filter '{op}' tidak berlaku untuk kolom ini.");
    }

    internal sealed class TextField<T>(Expression<Func<T, string?>> sel) : FilterField<T>
    {
        private static readonly MethodInfo ContainsMethod =
            typeof(string).GetMethod(nameof(string.Contains), [typeof(string)])!;

        public override Expression<Func<T, bool>> Build(FilterOp op, string raw)
        {
            var value = Param(raw.Trim());
            // NULL dianggap teks kosong, sama seperti tampilan di halaman.
            Expression body = Expression.Coalesce(sel.Body, Expression.Constant(""));

            Expression cond = op switch
            {
                FilterOp.Contains => Expression.Call(body, ContainsMethod, value),
                FilterOp.NotContains => Expression.Not(Expression.Call(body, ContainsMethod, value)),
                FilterOp.Equal => Expression.Equal(body, value),
                FilterOp.NotEqual => Expression.NotEqual(body, value),
                _ => throw BadOp(op),
            };
            return Expression.Lambda<Func<T, bool>>(cond, sel.Parameters);
        }
    }

    internal sealed class NumberField<T, TNum>(Expression<Func<T, TNum>> sel) : FilterField<T> where TNum : struct
    {
        public override Expression<Func<T, bool>> Build(FilterOp op, string raw)
        {
            if (!decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out var d))
                throw new FilterException($"Nilai angka tidak valid: {raw}");

            Expression left = typeof(TNum) == typeof(decimal) ? sel.Body : Expression.Convert(sel.Body, typeof(decimal));
            var right = Param(d);

            Expression cond = op switch
            {
                FilterOp.Equal => Expression.Equal(left, right),
                FilterOp.NotEqual => Expression.NotEqual(left, right),
                FilterOp.LessThan => Expression.LessThan(left, right),
                FilterOp.GreaterThan => Expression.GreaterThan(left, right),
                FilterOp.NotLessThan => Expression.GreaterThanOrEqual(left, right),
                FilterOp.NotGreaterThan => Expression.LessThanOrEqual(left, right),
                _ => throw BadOp(op),
            };
            return Expression.Lambda<Func<T, bool>>(cond, sel.Parameters);
        }
    }

    // Membandingkan per hari (jam diabaikan). Tanggal di database disimpan UTC.
    internal sealed class DateField<T>(Expression<Func<T, DateTime>> sel) : FilterField<T>
    {
        public override Expression<Func<T, bool>> Build(FilterOp op, string raw)
        {
            if (!DateTime.TryParseExact(raw, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                throw new FilterException($"Tanggal tidak valid (format yyyy-MM-dd): {raw}");

            var x = sel.Body;
            var day = Param(d.Date);
            var nextDay = Param(d.Date.AddDays(1));

            Expression onOrAfterDay = Expression.GreaterThanOrEqual(x, day);
            Expression beforeNextDay = Expression.LessThan(x, nextDay);

            Expression cond = op switch
            {
                FilterOp.Equal => Expression.AndAlso(onOrAfterDay, beforeNextDay),
                FilterOp.NotEqual => Expression.Not(Expression.AndAlso(onOrAfterDay, beforeNextDay)),
                FilterOp.LessThan => Expression.LessThan(x, day),
                FilterOp.GreaterThan => Expression.GreaterThanOrEqual(x, nextDay),
                FilterOp.NotLessThan => onOrAfterDay,
                FilterOp.NotGreaterThan => beforeNextDay,
                _ => throw BadOp(op),
            };
            return Expression.Lambda<Func<T, bool>>(cond, sel.Parameters);
        }
    }

    internal sealed class BoolField<T>(Expression<Func<T, bool>> sel) : FilterField<T>
    {
        public override Expression<Func<T, bool>> Build(FilterOp op, string raw)
        {
            if (op != FilterOp.Equal) throw BadOp(op);
            if (!bool.TryParse(raw, out var b)) throw new FilterException($"Nilai true/false tidak valid: {raw}");

            return Expression.Lambda<Func<T, bool>>(Expression.Equal(sel.Body, Param(b)), sel.Parameters);
        }
    }

    /// <summary>Pembuat kolom filter: FilterFields.Text&lt;Item&gt;(i => i.Name), dst.</summary>
    public static class FilterFields
    {
        public static FilterField<T> Text<T>(Expression<Func<T, string?>> sel) => new TextField<T>(sel);

        public static FilterField<T> Number<T, TNum>(Expression<Func<T, TNum>> sel) where TNum : struct =>
            new NumberField<T, TNum>(sel);

        public static FilterField<T> Date<T>(Expression<Func<T, DateTime>> sel) => new DateField<T>(sel);

        public static FilterField<T> Bool<T>(Expression<Func<T, bool>> sel) => new BoolField<T>(sel);
    }

    public static class FilterExtensions
    {
        /// <summary>Menerapkan semua filter ke query (menjadi WHERE di SQL). Hanya kolom yang didaftarkan yang boleh dipakai.</summary>
        public static IQueryable<T> ApplyFilters<T>(
            this IQueryable<T> query,
            IEnumerable<FilterCriterion>? filters,
            IReadOnlyDictionary<string, FilterField<T>> fields)
        {
            foreach (var f in filters ?? [])
            {
                if (string.IsNullOrWhiteSpace(f.Value)) continue;

                if (!fields.TryGetValue(f.Field, out var field))
                    throw new FilterException($"Kolom filter tidak dikenal: {f.Field}");

                query = query.Where(field.Build(f.Op, f.Value));
            }
            return query;
        }
    }
}