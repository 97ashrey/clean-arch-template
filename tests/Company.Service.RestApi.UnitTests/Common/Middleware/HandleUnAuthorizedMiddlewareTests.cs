using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using Company.Service.RestApi.Common.Middleware;

namespace Company.Service.RestApi.UnitTests.Common.Middleware;

public class HandleUnAuthorizedMiddlewareTests
{
    private readonly RequestDelegate _nextMiddleware;
    private readonly ProblemDetailsFactory _problemDetailsFactory;
    private readonly HandleUnauthorizedMiddleware _middleware;

    public HandleUnAuthorizedMiddlewareTests()
    {
        _nextMiddleware = Substitute.For<RequestDelegate>();
        _problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
        _middleware = new HandleUnauthorizedMiddleware(_nextMiddleware, _problemDetailsFactory);
    }

    [Fact]
    public async Task InvokeAsync_ShouldCallNextMiddleware()
    {
        // Arrange
        var context = CreateHttpContext(StatusCodes.Status200OK);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        await _nextMiddleware.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_With401Unauthorized_WithAuthenticationError_ShouldWriteProblemDetailsWithError()
    {
        // Arrange
        const string authError = "Invalid token";
        var context = CreateHttpContext(StatusCodes.Status401Unauthorized);
        context.Response.Headers.Append("WWW-Authenticate", authError);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = authError
        };

        _problemDetailsFactory.CreateProblemDetails(
                Arg.Any<HttpContext>(),
                detail: authError,
                statusCode: StatusCodes.Status401Unauthorized)
            .Returns(problemDetails);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Any<HttpContext>(),
            detail: authError,
            statusCode: StatusCodes.Status401Unauthorized);
        
        var responseBody = await ReadResponseBodyAsync(context);
        var deserializedDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        deserializedDetails.Should().NotBeNull();
        deserializedDetails!.Detail.Should().Be(authError);
    }

    [Fact]
    public async Task InvokeAsync_With401Unauthorized_WithBearerOnly_ShouldClearErrorDetail()
    {
        // Arrange
        const string bearer = "Bearer";
        var context = CreateHttpContext(StatusCodes.Status401Unauthorized);
        context.Response.Headers.Append("WWW-Authenticate", bearer);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized"
        };

        _problemDetailsFactory.CreateProblemDetails(
                Arg.Any<HttpContext>(),
                detail: default(StringValues),
                statusCode: StatusCodes.Status401Unauthorized)
            .Returns(problemDetails);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Any<HttpContext>(),
            detail: default(StringValues),
            statusCode: StatusCodes.Status401Unauthorized);
        
        var responseBody = await ReadResponseBodyAsync(context);
        var deserializedDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        deserializedDetails.Should().NotBeNull();
        deserializedDetails!.Detail.Should().BeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_With401Unauthorized_WithoutAuthenticationError_ShouldCreateProblemDetailsWithoutDetail()
    {
        // Arrange
        var context = CreateHttpContext(StatusCodes.Status401Unauthorized);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized"
        };

        _problemDetailsFactory.CreateProblemDetails(
                Arg.Any<HttpContext>(),
                detail: default(StringValues),
                statusCode: StatusCodes.Status401Unauthorized)
            .Returns(problemDetails);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Any<HttpContext>(),
            detail: default(StringValues),
            statusCode: StatusCodes.Status401Unauthorized);
        
        var responseBody = await ReadResponseBodyAsync(context);
        responseBody.Should().NotBeEmpty();
        var deserializedDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        deserializedDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task InvokeAsync_With403Forbidden_ShouldWriteProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext(StatusCodes.Status403Forbidden);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Forbidden"
        };

        _problemDetailsFactory.CreateProblemDetails(
                Arg.Any<HttpContext>(),
                statusCode: StatusCodes.Status403Forbidden)
            .Returns(problemDetails);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _problemDetailsFactory.Received(1).CreateProblemDetails(
            Arg.Any<HttpContext>(),
            statusCode: StatusCodes.Status403Forbidden);
        
        var responseBody = await ReadResponseBodyAsync(context);
        responseBody.Should().NotBeEmpty();
        var deserializedDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        deserializedDetails.Should().NotBeNull();
        deserializedDetails!.Status.Should().Be(StatusCodes.Status403Forbidden);
    }

    [Fact]
    public async Task InvokeAsync_With200OK_ShouldNotWriteProblemDetails()
    {
        // Arrange
        var context = CreateHttpContext(StatusCodes.Status200OK);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        _problemDetailsFactory.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task InvokeAsync_With401And403_ShouldResponseContainsProblemDetails()
    {
        // Arrange
        const string authError = "Token expired";
        var context = CreateHttpContext(StatusCodes.Status401Unauthorized);
        context.Response.Headers.Append("WWW-Authenticate", authError);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Detail = authError
        };

        _problemDetailsFactory.CreateProblemDetails(
                Arg.Any<HttpContext>(),
                detail: authError,
                statusCode: StatusCodes.Status401Unauthorized)
            .Returns(problemDetails);

        // Act
        await _middleware.InvokeAsync(context);

        // Assert
        var responseBody = await ReadResponseBodyAsync(context);
        responseBody.Should().NotBeEmpty();
        var deserializedDetails = JsonSerializer.Deserialize<ProblemDetails>(responseBody);
        deserializedDetails.Should().NotBeNull();
        deserializedDetails!.Status.Should().Be(StatusCodes.Status401Unauthorized);
        deserializedDetails!.Detail.Should().Be(authError);
    }

    private static HttpContext CreateHttpContext(int statusCode)
    {
        var context = new DefaultHttpContext();
        context.Response.StatusCode = statusCode;
        
        // Use a MemoryStream to capture the response body
        var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        return context;
    }

    private static async Task<string> ReadResponseBodyAsync(HttpContext context)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }
}
