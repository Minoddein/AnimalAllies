using System.Text;

namespace AnimalAllies.Core.Extension;

public static class SqlExtensions
{
    public static void ApplySorting(
        this StringBuilder sqlBuilder,
        string? sortBy,
        string? sortDirection,
        params string[] validSortBy)
    {
        if (!string.IsNullOrWhiteSpace(sortBy) && !string.IsNullOrWhiteSpace(sortDirection))
        {
            var validSortDirections = new[]{"asc", "desc"};

            if (validSortDirections.Contains(sortDirection?.ToLower()))
            {
                sqlBuilder.Append($"\norder by {sortBy} {sortDirection}");
            }
            else
            {
                throw new ArgumentException("Invalid sort parameters");
            }
        }
    }
    
    public static void ApplyPagination(
        this StringBuilder sqlBuilder,
        int page,
        int pageSize)
    {
        sqlBuilder.Append($"\nlimit {pageSize} offset {(page - 1) * pageSize}");
    }
    
    public static void ApplyBetweenFilter<TValue>(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        string? propertyName,
        TValue? valueFrom,
        TValue? valueTo)
    {
        if (!string.IsNullOrWhiteSpace(propertyName) && valueFrom != null && valueTo != null)
        {
            sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
            sqlBuilder.Append($"{propertyName} BETWEEN {valueFrom} AND {valueTo}");
            hasWhereClause = true;
        }
    }

    public static void ApplyFilterByValueFrom<TValue>(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        string? propertyName,
        TValue? valueFrom)
    {
        if (!string.IsNullOrWhiteSpace(propertyName) && valueFrom != null)
        {
            sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
            sqlBuilder.Append($"{propertyName} >= {valueFrom}");
            hasWhereClause = true;
        }
    }

    public static void ApplyFilterByValueTo<TValue>(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        string? propertyName,
        TValue? valueTo)
    {
        if (!string.IsNullOrWhiteSpace(propertyName) && valueTo != null)
        {
            sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
            sqlBuilder.Append($"{propertyName} <= {valueTo}");
            hasWhereClause = true;
        }
    }

    public static void ApplyFilterByString(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        Dictionary<string, string> properties)
    {
        foreach (var property in properties)
        {
            if (!string.IsNullOrWhiteSpace(property.Key) && !string.IsNullOrWhiteSpace(property.Value))
            {
                sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
                sqlBuilder.Append($"{property.Key} LIKE '%{property.Value}%'");
                hasWhereClause = true;
            }
        }
    }
    
    public static void ApplyFilterByEqualsValue<TValue>(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        string? propertyName,
        TValue? valueTo)
    {
        if (!string.IsNullOrWhiteSpace(propertyName) && valueTo != null)
        {
            sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
            sqlBuilder.Append($"{propertyName} = {valueTo}");
            hasWhereClause = true;
        }
    }
    
    public static void ApplySearhByEqualsValue<TValue>(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        string? propertyName,
        TValue? valueTo)
    {
        if (!string.IsNullOrWhiteSpace(propertyName) && valueTo != null)
        {
            sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
            sqlBuilder.Append($"{propertyName} ILIKE %{valueTo}%");
            hasWhereClause = true;
        }
    }
    
    public static void ApplySearchByContainsValue(
        this StringBuilder sqlBuilder,
        ref bool hasWhereClause,
        string columnName,
        string? searchTerm)
    {
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            sqlBuilder.Append(hasWhereClause ? " AND " : " WHERE ");
            sqlBuilder.Append($"{columnName} ILIKE @SearchTerm");
            hasWhereClause = true;
        }
    }
    
}