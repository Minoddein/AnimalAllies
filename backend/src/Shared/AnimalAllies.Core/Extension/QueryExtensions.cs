using System.Linq.Expressions;
using AnimalAllies.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AnimalAllies.Core.Extension;

public static class QueryExtensions
{
    public static async Task<PagedList<T>> ToPagedList<T>(
        this IQueryable<T> source,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        int totalCount = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        List<T> items = await source
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken).ConfigureAwait(false);

        return new PagedList<T> { Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount };
    }

    public static IQueryable<T> WhereIf<T>(
        this IQueryable<T> source,
        bool condition,
        Expression<Func<T, bool>> predicate) =>
        condition ? source.Where(predicate) : source;
}