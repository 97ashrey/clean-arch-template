using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using NSubstitute;
using Company.Service.RestApi.Common.Middleware;
using Microsoft.AspNetCore.Http;

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
