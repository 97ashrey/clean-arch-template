using AwesomeAssertions;
using Company.Service.RestApi.Common.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace Company.Service.RestApi.UnitTests.Common.Middleware;

public class HandleUnauthorizedMiddlewareExtensionsTests
{
    [Fact]
    public void UseHandleUnauthorized_ShouldRegisterHandleUnauthorizedMiddleware()
    {
        // Arrange
        var applicationBuilder = Substitute.For<IApplicationBuilder>();

        applicationBuilder.Use(Arg.Any<Func<RequestDelegate, RequestDelegate>>()).Returns(applicationBuilder);

        // Act
        var result = applicationBuilder.UseHandleUnauthorized();

        // Assert
        result.Should().NotBeNull();
        result.Should().Be(applicationBuilder);
    }
}