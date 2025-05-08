namespace AnimalAllies.Core.Models;

public class PagedList<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public int PageSize { get; init; }

    public int Page { get; init; }

    public long TotalCount { get; init; }

    public bool HasNextPage => Page * PageSize < TotalCount;

    public bool HasPreviousPage => Page * PageSize > 1;
}