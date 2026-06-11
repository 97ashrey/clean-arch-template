using Company.Service.Application.Common.Types;

namespace Company.Service.RestApi.Api;

public record class PagedResponse<T>
{
    public int CurrentPage { get; init; }
    public int CurrentPageCount => this.Items.Length;
    public int PageSize { get; init; }
    public int TotalPages { get; init; }
    public int TotalCount { get; init; }
    public T[] Items { get; init; } = [];
}

public static class PagedListMapping
{
    extension<T,TOut>(PagedList<T> pagedList)
    {
        public PagedResponse<TOut> ToPagedResponse(Func<T, TOut> map) =>
            new ()
            {
                CurrentPage = pagedList.CurrentPage,
                PageSize = pagedList.PageSize,
                TotalPages = pagedList.TotalPages,
                TotalCount = pagedList.TotalCount,
                Items = [.. pagedList.Items.Select(map)]
            };
    }
}