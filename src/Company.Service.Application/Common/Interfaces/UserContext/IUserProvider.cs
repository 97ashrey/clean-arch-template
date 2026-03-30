namespace Company.Service.Application.Common.Interfaces.UserContext;

public interface IUserProvider
{
    public Task<User?> GetCurrentUser();

    public async Task<User> GetCurrentUserOrDefault()
    {
        var user = await GetCurrentUser();

        if (user is null)
        {
            user = new User("0", "CompanyNamePlaceholder", "CompanyNamePlaceholder", "", "", 0);
        }

        return user;
    }
}
