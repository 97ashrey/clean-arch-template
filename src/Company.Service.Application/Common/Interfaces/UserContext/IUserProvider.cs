namespace Company.Service.Application.Common.Interfaces.UserContext;

public interface IUserProvider
{
    public Task<User?> GetCurrentUser();
}

internal static class UserProvivderExtensions
{
    private static readonly User DefaultUser = new User("0", "CompanyNamePlaceholder", "CompanyNamePlaceholder", "", "", 0);

    extension (IUserProvider userProvider)
    {
        public async Task<User> GetCurrentUserOrDefault()
        {
            var user = await userProvider.GetCurrentUser();

            return user ?? DefaultUser;
        }
    }
}