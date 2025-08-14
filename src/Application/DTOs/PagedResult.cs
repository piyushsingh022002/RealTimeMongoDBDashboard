namespace Application.DTOs;

public sealed class PagedResult<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public long Total { get; init; }
    public IEnumerable<T> Items { get; init; } = Array.Empty<T>();
}