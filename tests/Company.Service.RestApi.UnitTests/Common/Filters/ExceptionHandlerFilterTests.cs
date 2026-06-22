using AwesomeAssertions;
using Company.Service.RestApi.Common.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace Company.Service.RestApi.UnitTests.Common.Filters.Tests
{
    public class ExceptionHandlerFilterTests
    {
        private readonly ProblemDetailsFactory _problemDetailsFactory;
        private readonly FakeLogger<ExceptionHandlerFilter> _logger;
        private readonly ExceptionHandlerFilter _sut;

        public ExceptionHandlerFilterTests()
        {
            _problemDetailsFactory = Substitute.For<ProblemDetailsFactory>();
            _logger = new FakeLogger<ExceptionHandlerFilter>();

            _sut = new ExceptionHandlerFilter(_logger, _problemDetailsFactory);
        }

        [Fact]
        public void OnException_Should_Log_Error_With_Exception_And_Message()
        {
            // Arrange
            var context = CreateExceptionContext();

            var problemDetails = new ProblemDetails
            {
                Extensions = new Dictionary<string, object?>()
            };

            _problemDetailsFactory.CreateProblemDetails(
                    Arg.Any<HttpContext>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(problemDetails);

            // Act
            _sut.OnException(context);

            // Assert
            var log = _logger.Collector.LatestRecord;

            log.Level.Should().Be(LogLevel.Error);
            log.Exception.Should().Be(context.Exception);
            log.Message.Should().Be("An unexpected error has occured.");
        }

        [Fact]
        public void OnException_Should_Create_ProblemDetails_With_Expected_Values()
        {
            // Arrange
            var context = CreateExceptionContext();

            var problemDetails = new ProblemDetails
            {
                Extensions = new Dictionary<string, object?>()
            };

            _problemDetailsFactory.CreateProblemDetails(
                    Arg.Any<HttpContext>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(problemDetails);

            // Act
            _sut.OnException(context);

            // Assert
            _problemDetailsFactory.Received(1).CreateProblemDetails(
                context.HttpContext,
                StatusCodes.Status500InternalServerError,
                null,
                null,
                "An unexpected error has occured.",
                null
                );
        }

        [Fact]
        public void OnException_Should_Set_JsonResult_With_500_Status_And_ProblemDetails()
        {
            // Arrange
            var context = CreateExceptionContext();

            var problemDetails = new ProblemDetails
            {
                Extensions = new Dictionary<string, object?>()
            };

            _problemDetailsFactory.CreateProblemDetails(
                    Arg.Any<HttpContext>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(problemDetails);

            // Act
            _sut.OnException(context);

            // Assert
            context.Result.Should().BeOfType<JsonResult>();

            var result = (JsonResult)context.Result!;
            result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            result.Value.Should().Be(problemDetails);
        }

        [Fact]
        public void OnException_Should_Mark_Exception_As_Handled()
        {
            // Arrange
            var context = CreateExceptionContext();

            var problemDetails = new ProblemDetails
            {
                Extensions = new Dictionary<string, object?>()
            };

            _problemDetailsFactory.CreateProblemDetails(
                    Arg.Any<HttpContext>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(problemDetails);

            // Act
            _sut.OnException(context);

            // Assert
            context.ExceptionHandled.Should().BeTrue();
        }

        [Fact]
        public void OnException_Should_Add_ErrorId_To_ProblemDetails()
        {
            // Arrange
            var context = CreateExceptionContext();

            var problemDetails = new ProblemDetails
            {
                Extensions = new Dictionary<string, object?>()
            };

            _problemDetailsFactory.CreateProblemDetails(
                    Arg.Any<HttpContext>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(problemDetails);

            // Act
            _sut.OnException(context);

            // Assert
            problemDetails.Extensions.Should().ContainKey("errorId");

            var errorId = problemDetails.Extensions["errorId"];
            errorId.Should().BeOfType<string>();
            errorId.As<string>().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void OnException_Should_Include_ErrorId_In_Logging_Scope()
        {
            // Arrange
            var context = CreateExceptionContext();

            var problemDetails = new ProblemDetails
            {
                Extensions = new Dictionary<string, object?>()
            };

            _problemDetailsFactory.CreateProblemDetails(
                    Arg.Any<HttpContext>(),
                    Arg.Any<int?>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string>())
                .Returns(problemDetails);

            // Act
            _sut.OnException(context);

            // Assert
            var log = _logger.Collector.LatestRecord;

            log.Scopes.Should().NotBeNull();
            log.Scopes.Should().ContainSingle();

            var scope = log.Scopes[0] as Dictionary<string, object>;

            scope!.Should().Contain(kvp => kvp.Key == "ErrorId");
        }

        private static ExceptionContext CreateExceptionContext()
        {
            var httpContext = new DefaultHttpContext();

            var actionContext = new ActionContext
            {
                RouteData = new(),
                ActionDescriptor = new(),
                HttpContext = httpContext
            };

            return new ExceptionContext(actionContext, [])
            {
                Exception = new Exception("Test exception")
            };
        }
    }
}