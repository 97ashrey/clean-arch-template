namespace Company.Service.Application.Common.Types;

public class PagedList<T>
{
    public int CurrentPage { get; }
    public int CurrentPageCount => this.items.Length;
    public int PageSize { get; }
    public int TotalPages { get; }
    public int TotalCount { get; }

    private readonly T[] items = [];

    public PagedList(T[] items, int currentPage, int pageSize, int totalCount)
    {
        this.items = items;
        this.CurrentPage = currentPage;
        this.PageSize = pageSize;
        this.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        this.TotalCount = totalCount;
    }
}