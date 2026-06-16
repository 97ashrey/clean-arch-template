namespace Company.Service.Application.Common.Utils;

internal static class TimeProviderExtensions
{
    extension(TimeProvider timeProvider)
    {
        public DateTime GetUtcNowDateTime()
        {
            return timeProvider.GetUtcNow().DateTime;
        }
    }
}