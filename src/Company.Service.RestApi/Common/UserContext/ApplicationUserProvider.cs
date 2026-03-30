using Company.Service.Application.Common.Interfaces.UserContext;
using System.Text.Json;

namespace Company.Service.RestApi.Common.UserContext;

internal class ApplicationUserProvider : IUserProvider
{
    private const string USER_HEADER = "X-USER";

    private readonly IHttpContextAccessor _contextAccessor;
    private readonly ILogger<ApplicationUserProvider> _logger;

    public ApplicationUserProvider(IHttpContextAccessor contextAccessor, ILogger<ApplicationUserProvider> logger)
    {
        _contextAccessor = contextAccessor;
        _logger = logger;
    }

    public async Task<User?> GetCurrentUser()
    {
        return await Task.FromResult(GetUserFromHeaders());
    }

    private User? GetUserFromHeaders()
    {
        var request = _contextAccessor.HttpContext?.Request;

        if (request is null)
        {
            return null;
        }


        if (!request.Headers.TryGetValue(USER_HEADER, out var value))
        {
            _logger.LogWarning("Missing header {header}. Can't construct a valid user context.", USER_HEADER);
            
            return null;
        }

        try
        {
            var user = JsonSerializer.Deserialize<User>(value!);
            
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Can't deserialize the user value {userValue}!", value!);

            return null;            
        }
    }
}