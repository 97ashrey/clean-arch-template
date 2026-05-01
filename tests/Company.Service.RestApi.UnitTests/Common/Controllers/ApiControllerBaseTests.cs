using Company.Service.Application.Common.Types.Errors;
using Company.Service.RestApi.Common.Controllers;
using FluentAssertions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Company.Service.RestApi.UnitTests.Common.Controllers;

public class ApiControllerBaseTests
{
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly IMediator _mediatorMock = Substitute.For<IMediator>();
    private readonly FakeLogger<ApiControllerBase> _fakeLogger = new FakeLogger<ApiControllerBase>();
    private readonly HttpContext _httpContextMock = Substitute.For<HttpContext>();
    private readonly ProblemDetailsFactory _problemDetailsFactoryMock = Substitute.For<ProblemDetailsFactory>();
    private readonly TestApiController _sut;

    public ApiControllerBaseTests()
    {
        _httpContextMock.RequestServices.Returns(_serviceProvider);

        _serviceProvider.GetService(typeof(IMediator)).Returns(_mediatorMock);
        _serviceProvider.GetService(typeof(ILogger<ApiControllerBase>)).Returns(_fakeLogger);

        _sut = new TestApiController { ControllerContext = new ControllerContext { HttpContext = _httpContextMock }, ProblemDetailsFactory = _problemDetailsFactoryMock };
    }

    [Fact]
    public void Mediator_Property_LazilyInitializesMediator()
    {
        // Act
        var mediator = _sut.Mediator;

        // Assert
        mediator.Should().NotBeNull();
        mediator.Should().Be(_mediatorMock);
    }

    [Fact]
    public void Mediator_Property_ReturnsTheSameInstanceOnMultipleCalls()
    {
        // Act
        var firstCall = _sut.Mediator;
        var secondCall = _sut.Mediator;

        // Assert
        firstCall.Should().Be(secondCall);
    }

    [Fact]
    public void Logger_Property_LazilyInitializesLogger()
    {
        // Act
        var logger = _sut.Logger;

        // Assert
        logger.Should().NotBeNull();
        logger.Should().Be(_fakeLogger);
    }

    [Fact]
    public void Logger_Property_ReturnsTheSameInstanceOnMultipleCalls()
    {
        // Act
        var firstCall = _sut.Logger;
        var secondCall = _sut.Logger;

        // Assert
        firstCall.Should().Be(secondCall);
    }

    [Fact]
    public void NotFoundProblemResponse_WithNotFoundError_ReturnsNotFoundResult()
    {
        // Arrange
        var notFoundError = new NotFoundError { Message = "Resource not found" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Detail = "Resource not found"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.NotFoundProblemResponseTest(notFoundError);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        result.Value.Should().NotBeNull();
        result.Value!.Detail.Should().Be("Resource not found");
        _problemDetailsFactoryMock.Received(1).CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public void NotFoundProblemResponse_WithNotFoundError_ProblemDetailsContainsErrorId()
    {
        // Arrange
        var notFoundError = new NotFoundError { Message = "Resource not found" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Detail = "Resource not found"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.NotFoundProblemResponseTest(notFoundError);

        // Assert
        result.Value!.Extensions.Should().ContainKey("errorId");
        result.Value.Extensions["errorId"].Should().Be(notFoundError.Id);
        result.Value.Extensions["errorId"].Should().BeOfType<Guid>();
    }

    [Fact]
    public void NotFoundProblemResponse_WithEmptyMessage_ReturnsProblemDetails()
    {
        // Arrange
        var notFoundError = new NotFoundError { Message = "" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Detail = ""
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.NotFoundProblemResponseTest(notFoundError);

        // Assert
        result.Value!.Detail.Should().BeEmpty();
        result.Value.Extensions.Should().ContainKey("errorId");
    }

    [Fact]
    public void NotFoundProblemResponse_WithComplexMessage_ReturnsProblemDetails()
    {
        // Arrange
        var message = "Resource with ID 12345 not found in database";
        var notFoundError = new NotFoundError { Message = message };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Detail = message
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.NotFoundProblemResponseTest(notFoundError);

        // Assert
        result.Value!.Detail.Should().Be(message);
        result.Value.Extensions.Should().ContainKey("errorId");
        result.Value.Extensions["errorId"].Should().Be(notFoundError.Id);
    }

    [Fact]
    public void BadRequestProblemResponse_WithBadRequestError_ReturnsBadRequestResult()
    {
        // Arrange
        var badRequestError = new BadRequestError { Message = "Invalid request" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = "Invalid request"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.BadRequestProblemResponseTest(badRequestError);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Value.Should().NotBeNull();
        result.Value!.Detail.Should().Be("Invalid request");
        result.Value.Extensions.Should().ContainKey("errorId");
    }

    [Fact]
    public void BadRequestProblemResponse_WithBadRequestError_ProblemDetailsContainsErrorId()
    {
        // Arrange
        var badRequestError = new BadRequestError { Message = "Invalid request" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = "Invalid request"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.BadRequestProblemResponseTest(badRequestError);

        // Assert
        result.Value!.Extensions.Should().ContainKey("errorId");
        result.Value.Extensions["errorId"].Should().Be(badRequestError.Id);
        result.Value.Extensions["errorId"].Should().BeOfType<Guid>();
    }

    [Fact]
    public void BadRequestProblemResponse_WithMultipleErrors_ReturnsProblemDetails()
    {
        // Arrange
        var message = "Invalid input: Field1 is required, Field2 must be positive";
        var badRequestError = new BadRequestError { Message = message };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = message
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.BadRequestProblemResponseTest(badRequestError);

        // Assert
        result.Value!.Detail.Should().Be(message);
        result.Value.Extensions["errorId"].Should().Be(badRequestError.Id);
    }

    [Fact]
    public void ValidationProblemResponse_WithValidationError_ReturnsBadRequestResult()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Name", ["Name is required"]),
            new ValidationFailure("Email", ["Email is invalid"])
        };
        var validationError = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };
        var validationProblemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>())
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = "Validation failed"
        };
        _problemDetailsFactoryMock.CreateValidationProblemDetails(
            _httpContextMock,
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(validationProblemDetails);

        // Act
        var result = _sut.ValidationProblemResponseTest(validationError);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        result.Value.Should().NotBeNull();
        result.Value!.Detail.Should().Be("Validation failed");
        _problemDetailsFactoryMock.Received(1).CreateValidationProblemDetails(
            _httpContextMock,
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>());
    }

    [Fact]
    public void ValidationProblemResponse_WithValidationError_PopulatesValidationErrors()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Name", ["Name is required", "Name too long"]),
            new ValidationFailure("Email", ["Email is invalid"])
        };
        var validationError = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };
        var validationProblemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>())
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = "Validation failed"
        };
        _problemDetailsFactoryMock.CreateValidationProblemDetails(
            _httpContextMock,
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(validationProblemDetails);

        // Act
        var result = _sut.ValidationProblemResponseTest(validationError);

        // Assert
        result.Value!.Errors.Should().HaveCount(2);
        result.Value!.Errors.Should().ContainKey("Name");
        result.Value!.Errors.Should().ContainKey("Email");
        result.Value!.Errors["Name"].Should().Equal("Name is required", "Name too long");
        result.Value!.Errors["Email"].Should().Equal("Email is invalid");
    }

    [Fact]
    public void ValidationProblemResponse_WithMultipleFailuresPerProperty_PopulatesAllErrors()
    {
        // Arrange
        var failures = new[]
        {
            new ValidationFailure("Password", ["Password is required", "Password must be at least 8 characters", "Password must contain uppercase"])
        };
        var validationError = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };
        var validationProblemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>())
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = "Validation failed"
        };
        _problemDetailsFactoryMock.CreateValidationProblemDetails(
            _httpContextMock,
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(validationProblemDetails);

        // Act
        var result = _sut.ValidationProblemResponseTest(validationError);

        // Assert
        result.Value!.Errors["Password"].Should().HaveCount(3);
        result.Value!.Errors["Password"].Should().Equal(
            "Password is required",
            "Password must be at least 8 characters",
            "Password must contain uppercase"
        );
    }

    [Fact]
    public void ValidationProblemResponse_WithEmptyFailures_ReturnsProblemDetails()
    {
        // Arrange
        var validationError = new ValidationError
        {
            Message = "Validation failed",
            Failures = []
        };
        var validationProblemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>())
        {
            Status = StatusCodes.Status400BadRequest,
            Detail = "Validation failed"
        };
        _problemDetailsFactoryMock.CreateValidationProblemDetails(
            _httpContextMock,
            Arg.Any<ModelStateDictionary>(),
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(validationProblemDetails);

        // Act
        var result = _sut.ValidationProblemResponseTest(validationError);

        // Assert
        result.Value!.Errors.Should().BeEmpty();
    }

    [Fact]
    public void InternalServerErrorProblemResponse_WithApplicationError_ReturnsInternalServerErrorResult()
    {
        // Arrange
        var applicationError = new ApplicationError { Message = "Internal server error" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Internal server error"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.InternalServerErrorProblemResponseTest(applicationError);

        // Assert
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        result.Value.Should().NotBeNull();
        result.Value!.Detail.Should().Be("Internal server error");
        result.Value.Extensions.Should().ContainKey("errorId");
    }

    [Fact]
    public void InternalServerErrorProblemResponse_WithApplicationError_ProblemDetailsContainsErrorId()
    {
        // Arrange
        var applicationError = new ApplicationError { Message = "Internal server error" };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Internal server error"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.InternalServerErrorProblemResponseTest(applicationError);

        // Assert
        result.Value!.Extensions.Should().ContainKey("errorId");
        result.Value!.Extensions["errorId"].Should().Be(applicationError.Id);
        result.Value!.Extensions["errorId"].Should().BeOfType<Guid>();
    }

    [Fact]
    public void InternalServerErrorProblemResponse_WithValidationError_ReturnsProblemDetailsWithErrorId()
    {
        // Arrange - ValidationError is also an ApplicationError
        var failures = new[] { new ValidationFailure("Field", ["Error"]) };
        var validationError = new ValidationError
        {
            Message = "Validation failed",
            Failures = failures
        };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Validation failed"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.InternalServerErrorProblemResponseTest(validationError);

        // Assert
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        result.Value!.Extensions.Should().ContainKey("errorId");
        result.Value!.Extensions["errorId"].Should().Be(validationError.Id);
        result.Value!.Extensions["errorId"].Should().BeOfType<Guid>();
    }

    [Fact]
    public void ErrorIdExtension_IsConsistent_AcrossAllErrorResponse()
    {
        // Arrange
        var errorId = Guid.NewGuid();
        var notFoundError = new NotFoundError { Message = "Not found", Id = errorId };
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Detail = "Not found"
        };
        _problemDetailsFactoryMock.CreateProblemDetails(
            _httpContextMock,
            Arg.Any<int?>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(problemDetails);

        // Act
        var result = _sut.NotFoundProblemResponseTest(notFoundError);

        // Assert
        var errorIdFromResponse = (Guid?)result.Value!.Extensions["errorId"];
        errorIdFromResponse.Should().Be(errorId);
        result.Value.Extensions.Should().ContainKey("errorId");
        _problemDetailsFactoryMock.Received(1).CreateProblemDetails(
            _httpContextMock,
            StatusCodes.Status404NotFound,
            Arg.Any<string>(),
            Arg.Any<string>(),
            "Not found",
            Arg.Any<string>());
    }

    /// <summary>
    /// Test controller that exposes protected methods for testing
    /// </summary>
    private class TestApiController : ApiControllerBase
    {
        public new IMediator Mediator => base.Mediator;
        public new ILogger Logger => base.Logger;

        public NotFound<ProblemDetails> NotFoundProblemResponseTest(NotFoundError error) =>
            base.NotFoundProblemResponse(error);

        public BadRequest<ProblemDetails> BadRequestProblemResponseTest(BadRequestError error) =>
            base.BadRequestProblemResponse(error);

        public BadRequest<ValidationProblemDetails> ValidationProblemResponseTest(ValidationError error) =>
            base.ValidationproblemResponse(error);

        public InternalServerError<ProblemDetails> InternalServerErrorProblemResponseTest(ApplicationError error) =>
            base.InternalServerErrorProblemResponse(error);
    }
}