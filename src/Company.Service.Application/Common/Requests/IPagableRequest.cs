namespace Company.Service.Application.Common.Requests;

internal interface IPagableRequest
{
    public int PageNumber { get; init; }

    public int PageSize { get; init; }
}

internal static class PagableRequestExtensions
{
    extension(IPagableRequest request)
    {
        public int GetPageNumberOrDefault(int defaultPageNumber = 1) =>
            request.PageNumber > 0 ? request.PageNumber : defaultPageNumber;

        public int GetPageSizeOrDefault(int defaultPageSize = 10) =>
            request.PageSize > 0 ? request.PageSize : defaultPageSize;
    }
}