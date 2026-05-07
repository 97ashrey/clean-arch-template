using Company.Service.Application.Common.Behaviours;
using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Company.Service.Domain.Common.Types;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;

namespace Company.Service.Application.UnitTests.Common.Behaviours;

public class ExceptionHandlerPipelineBehaviourTests
{
    private readonly FakeLogger<ExceptionHandlerPipelineBehaviour<FakeApiRequest, ValueResult<string, ApplicationError>>> fakeLogger = new();

    private readonly ExceptionHandlerPipelineBehaviour<FakeApiRequest, ValueResult<string, ApplicationError>> _sut;

    public ExceptionHandlerPipelineBehaviourTests()
    {
        _sut = new(fakeLogger);
    }

    private record FakeApiRequest : ApplicationRequest<string>;

    [Fact]
    public async Task Handle_WhenExceptionIsCaught_LogsItAndReturnsApplicationError()
    {
        // Arrange
        var ex = new Exception("Nasty error!");

        // Act
        var result = await _sut.Handle(new FakeApiRequest(), (_, _) => throw ex, default);

        // Assert
        result.IsSuccess.Should().BeFalse();

        var error = result.Error!;

        error.Should().NotBeNull();
        error.Message.Should().Be("An unexpected error has occured.");
        error.Id.Should().NotBeEmpty();

        fakeLogger.LatestRecord.Level.Should().Be(LogLevel.Error);
        fakeLogger.LatestRecord.Message.Should().Be("An unexpected error has occured.");
        fakeLogger.LatestRecord.Exception.Should().Be(ex);
    }

    [Fact]
    public async Task Handle_WhenNoExceptionOccurs_ReturnsSuccessfulResult()
    {
        // Arrange
        var expectedResult = ValueResult<string, ApplicationError>.Success("Success");

        // Act
        var result = await _sut.Handle(
            new FakeApiRequest(),
            (_, _) => ValueTask.FromResult(expectedResult),
            default);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Success");
        fakeLogger.Collector.Count.Should().Be(0, "Because the request was successfull and nothing should be logged");
    }
}