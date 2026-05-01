using Company.Service.Application.Common.Interfaces.UserContext;
using Company.Service.RestApi.Common.UserContext;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Text.Json;

namespace Company.Service.RestApi.UnitTests.Common.UserContext;

public class ApplicationUserProviderTests
{
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly FakeLogger<ApplicationUserProvider> _logger;
    private readonly ApplicationUserProvider _sut;

    public ApplicationUserProviderTests()
    {
        _contextAccessor = Substitute.For<IHttpContextAccessor>();
        _logger = new FakeLogger<ApplicationUserProvider>();
        _sut = new ApplicationUserProvider(_contextAccessor, _logger);
    }

    [Fact]
    public async Task GetCurrentUser_WithValidUserHeader_ReturnsUser()
    {
        // Arrange
        var user = new User("123", "john.doe", "John", "Doe", "john@example.com", 1);
        var userJson = JsonSerializer.Serialize(user);

        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary { { "X-USER", userJson } };

        request.Headers.Returns(headers);
        httpContext.Request.Returns(request);
        _contextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = await _sut.GetCurrentUser();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(user);
    }

    [Fact]
    public async Task GetCurrentUser_WithMissingUserHeader_ReturnsNull()
    {
        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary();

        request.Headers.Returns(headers);
        httpContext.Request.Returns(request);
        _contextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = await _sut.GetCurrentUser();

        // Assert
        result.Should().BeNull();
        _logger.LatestRecord.Level.Should().Be(LogLevel.Warning);
        _logger.LatestRecord.Message.Should().Contain("Missing header");
        _logger.Collector.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentUser_WithInvalidJsonInHeader_ReturnsNull()
    {
        // Arrange
        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary { { "X-USER", "invalid-json" } };

        request.Headers.Returns(headers);
        httpContext.Request.Returns(request);
        _contextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = await _sut.GetCurrentUser();

        // Assert
        result.Should().BeNull();
        _logger.LatestRecord.Level.Should().Be(LogLevel.Error);
        _logger.LatestRecord.Message.Should().Contain("Can't deserialize");
        _logger.LatestRecord.Exception.Should().NotBeNull();
        _logger.Collector.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetCurrentUser_WithNullHttpContext_ReturnsNull()
    {
        // Arrange
        _contextAccessor.HttpContext.Returns((HttpContext?)null);

        // Act
        var result = await _sut.GetCurrentUser();

        // Assert
        result.Should().BeNull();
        _logger.Collector.Count.Should().Be(0, "Because HttpContext is null, nothing should be logged");
    }

    [Fact]
    public async Task GetCurrentUser_WithComplexUserData_ReturnsUserCorrectly()
    {
        // Arrange
        var user = new User(
            "user-guid-12345",
            "jane.smith",
            "Jane",
            "Smith",
            "jane.smith@company.com",
            42);
        var userJson = JsonSerializer.Serialize(user);

        var httpContext = Substitute.For<HttpContext>();
        var request = Substitute.For<HttpRequest>();
        var headers = new HeaderDictionary { { "X-USER", userJson } };

        request.Headers.Returns(headers);
        httpContext.Request.Returns(request);
        _contextAccessor.HttpContext.Returns(httpContext);

        // Act
        var result = await _sut.GetCurrentUser();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("user-guid-12345");
        result.UserName.Should().Be("jane.smith");
        result.FirstName.Should().Be("Jane");
        result.LastName.Should().Be("Smith");
        result.Email.Should().Be("jane.smith@company.com");
        result.TenantId.Should().Be(42);
    }
}