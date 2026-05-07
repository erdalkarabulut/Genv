using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace NArchitecture.Core.Persistence.Dynamic;

public static class IQueryableDynamicFilterExtensions
{
    private static readonly ParsingConfig s_dynamicLinqParsingConfig = new()
    {
        CustomTypeProvider = new EfCoreDynamicLinqCustomTypeProvider()
    };

    private sealed class EfCoreDynamicLinqCustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        public EfCoreDynamicLinqCustomTypeProvider()
            : base(cacheCustomTypes: true)
        {
        }

        public override HashSet<Type> GetCustomTypes()
        {
            HashSet<Type>? fromBase = base.GetCustomTypes();
            HashSet<Type> types = fromBase is null ? new HashSet<Type>() : new HashSet<Type>(fromBase);
            types.Add(typeof(EF));
            types.Add(typeof(DbFunctionsExtensions));
            types.Add(typeof(NpgsqlDbFunctionsExtensions));
            return types;
        }
    }

    private static readonly string[] _orders = { "asc", "desc" };
    private static readonly string[] _logics = { "and", "or" };

    private static readonly IDictionary<string, string> _operators = new Dictionary<string, string>
    {
        { "eq", "=" },
        { "neq", "!=" },
        { "lt", "<" },
        { "lte", "<=" },
        { "gt", ">" },
        { "gte", ">=" },
        { "isnull", "== null" },
        { "isnotnull", "!= null" },
        { "startswith", "StartsWith" },
        { "endswith", "EndsWith" },
        { "contains", "Contains" },
        { "doesnotcontain", "Contains" }
    };

    public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
    {
        if (dynamicQuery.Filter is not null)
            query = Filter(query, dynamicQuery.Filter);
        if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any())
            query = Sort(query, dynamicQuery.Sort);
        return query;
    }

    private static IQueryable<T> Filter<T>(IQueryable<T> queryable, Filter filter)
    {
        IList<Filter> filters = GetAllFilters(filter);
        object?[] values = filters.Select(f => (object?)f.Value).ToArray();
        for (int i = 0; i < filters.Count; i++)
        {
            if (filters[i].Value is not string stringValue)
                continue;

            string op = filters[i].Operator ?? string.Empty;
            if (string.Equals(op, "contains", StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, "doesnotcontain", StringComparison.OrdinalIgnoreCase))
            {
                values[i] = toPostgresIlikeContainsPattern(stringValue);
            }
            else if (string.Equals(op, "startswith", StringComparison.OrdinalIgnoreCase))
            {
                values[i] = toPostgresIlikeStartsWithPattern(stringValue);
            }
            else if (string.Equals(op, "endswith", StringComparison.OrdinalIgnoreCase))
            {
                values[i] = toPostgresIlikeEndsWithPattern(stringValue);
            }
            else if (string.Equals(op, "eq", StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, "neq", StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, "gt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, "gte", StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, "lt", StringComparison.OrdinalIgnoreCase)
                || string.Equals(op, "lte", StringComparison.OrdinalIgnoreCase))
            {
                values[i] = coerceScalarForDynamicLinqComparison(stringValue);
            }
        }

        string where = Transform(filter, filters);
        if (!string.IsNullOrEmpty(where) && values is not null)
            queryable = queryable.Where(s_dynamicLinqParsingConfig, where, Array.ConvertAll(values, v => v!));

        return queryable;
    }

    private static string toPostgresIlikeContainsPattern(string value) =>
        "%" + escapePostgresLikeWildcards(value) + "%";

    private static string toPostgresIlikeStartsWithPattern(string value) =>
        escapePostgresLikeWildcards(value) + "%";

    private static string toPostgresIlikeEndsWithPattern(string value) =>
        "%" + escapePostgresLikeWildcards(value);

    private static string escapePostgresLikeWildcards(string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return value
            .Replace("\\", "\\\\\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);
    }

    private static object coerceScalarForDynamicLinqComparison(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return raw;

        if (Guid.TryParse(raw, out Guid guid))
            return guid;

        if (bool.TryParse(raw, out bool boolean))
            return boolean;

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset dto))
            return dto;

        if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime dt))
            return dt;

        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out long lng))
        {
            if (lng is >= int.MinValue and <= int.MaxValue)
                return (int)lng;
            return lng;
        }

        if (decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal dec))
            return dec;

        return raw;
    }

    private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
    {
        foreach (Sort item in sort)
        {
            if (string.IsNullOrEmpty(item.Field))
                throw new ArgumentException("Invalid Field");
            if (string.IsNullOrEmpty(item.Dir) || !_orders.Contains(item.Dir))
                throw new ArgumentException("Invalid Order Type");
        }

        if (sort.Any())
        {
            string ordering = string.Join(separator: ",", values: sort.Select(s => $"{s.Field} {s.Dir}"));
            return queryable.OrderBy(ordering);
        }

        return queryable;
    }

    public static IList<Filter> GetAllFilters(Filter filter)
    {
        List<Filter> filters = new List<Filter>();
        GetFilters(filter, filters);
        return filters;
    }

    private static void GetFilters(Filter filter, IList<Filter> filters)
    {
        filters.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter item in filter.Filters)
                GetFilters(item, filters);
    }

    public static string Transform(Filter filter, IList<Filter> filters)
    {
        // A composite filter has child filters — it represents a logic group, not a leaf condition.
        bool isComposite = filter.Filters is not null && filter.Filters.Any();

        if (isComposite)
        {
            // Composite nodes don't have field/operator — only validate logic
            if (string.IsNullOrEmpty(filter.Logic) || !_logics.Contains(filter.Logic))
                throw new ArgumentException("Invalid Logic");

            string childWhere = string.Join($" {filter.Logic} ", filter.Filters.Select(f => Transform(f, filters)));
            return $"({childWhere})";
        }

        // Leaf filter — validate field and operator
        if (string.IsNullOrEmpty(filter.Field))
            throw new ArgumentException("Invalid Field");
        if (string.IsNullOrEmpty(filter.Operator) || !_operators.ContainsKey(filter.Operator))
            throw new ArgumentException("Invalid Operator");

        int index = filters.IndexOf(filter);
        string comparison = _operators[filter.Operator];
        StringBuilder where = new();

        if (!string.IsNullOrEmpty(filter.Value))
        {
            if (filter.Operator == "doesnotcontain")
                where.Append($"(!EF.Functions.ILike({filter.Field}, @{index}))");
            else if (comparison is "StartsWith" or "EndsWith" or "Contains")
                where.Append($"(EF.Functions.ILike({filter.Field}, @{index}))");
            else
                where.Append($"{filter.Field} {comparison} @{index}");
        }
        else if (filter.Operator is "isnull" or "isnotnull")
        {
            where.Append($"{filter.Field} {comparison}");
        }

        return where.ToString();
    }
}