using Company.Service.Application.Common.Behaviours;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Company.Service.Application.UnitTests.Common.Behaviours;

public class ResultLoggingPipelineBehaviourTests
{
    private readonly FakeLogger<ResultLoggingPipelineBehaviour<FakeApiRequest, Result<string, ApplicationError>>> fakeLogger = new();

    private readonly ResultLoggingPipelineBehaviour<FakeApiRequest, Result<string, ApplicationError>> _sut;

    public ResultLoggingPipelineBehaviourTests()
    {
        _sut = new(fakeLogger);
    }

    private record FakeApiRequest : ApplicationRequest<string>;

    [Fact]
    public async Task Handle_WhenResultContainsError_LogsTheErrorAndReturnsResult()
    {
        // Arrange
        var error = new ApplicationError { Message = "Something went wrong" };
        var expectedResult = Result<string, ApplicationError>.Failure(error);

        // Act
        var result = await _sut.Handle(
            new FakeApiRequest(),
            (_, _) => ValueTask.FromResult(expectedResult),
            default);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);

        fakeLogger.LatestRecord.Level.Should().Be(LogLevel.Error);
        fakeLogger.LatestRecord.Message.Should().Contain("Application responded with an error:");
        fakeLogger.Collector.Count.Should().Be(1, "Because the request failed with one error");
    }

    [Fact]
    public async Task Handle_WhenResultIsSuccessful_DoesNotLogAndReturnsResult()
    {
        // Arrange
        var expectedResult = Result<string, ApplicationError>.Success("Success");

        // Act
        var result = await _sut.Handle(
            new FakeApiRequest(),
            (_, _) => ValueTask.FromResult(expectedResult),
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
        fakeLogger.Collector.Count.Should().Be(0, "Because the request was successful and nothing should be logged");
    }
}
