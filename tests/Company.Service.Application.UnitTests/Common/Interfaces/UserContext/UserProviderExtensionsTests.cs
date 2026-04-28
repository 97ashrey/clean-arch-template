using Company.Service.Application.Common.Interfaces.UserContext;
using FluentAssertions;

namespace Company.Service.Application.UnitTests.Common.Interfaces.UserContext;

public class UserProviderExtensionsTests
{
    [Fact]
    public async Task GetCurrentUserOrDefault_WhenCurrentUserExists_ReturnsCurrentUser()
    {
        // Arrange
        var expectedUser = new User("123", "John", "Acme Corp", "john@acme.com", "password", 1);
        IUserProvider provider = new TestUserProvider(expectedUser);

        // Act
        var result = await provider.GetCurrentUserOrDefault();

        // Assert
        // Covering the User object with tests
        result.Id.Should().Be(expectedUser.Id);
        result.TenantId.Should().Be(expectedUser.TenantId);
        result.UserName.Should().Be(expectedUser.UserName);
        result.FirstName.Should().Be(expectedUser.FirstName);
        result.LastName.Should().Be(expectedUser.LastName);
        result.Email.Should().Be(expectedUser.Email);
        result.FullName.Should().Be(expectedUser.FullName);
    }

    [Fact]
    public async Task GetCurrentUserOrDefault_WhenCurrentUserIsNull_ReturnsDefaultUser()
    {
        // Arrange
        IUserProvider provider = new TestUserProvider(null);

        var expectedUser = new User("0", "CompanyNamePlaceholder", "CompanyNamePlaceholder", "", "", 0);

        // Act
        var result = await provider.GetCurrentUserOrDefault();

        // Assert
        result.Should().Be(expectedUser);
    }

    private class TestUserProvider : IUserProvider
    {
        private readonly User? _currentUser;

        public TestUserProvider(User? currentUser)
        {
            _currentUser = currentUser;
        }

        public Task<User?> GetCurrentUser()
        {
            return Task.FromResult(_currentUser);
        }
    }
}
