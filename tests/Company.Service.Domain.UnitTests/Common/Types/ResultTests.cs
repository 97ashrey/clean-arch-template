using Company.Service.Domain.Common.Types;
using FluentAssertions;

namespace Company.Service.Domain.UnitTests.Common.Types;

public class ResultTests
{
    #region Result<TError> Constructor and Factory Tests

    [Fact]
    public void DefaultConstructor_CreatesSuccessResult()
    {
        // Act
        var result = new Result<string>();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void ConstructorWithError_CreatesFailureResult()
    {
        // Arrange
        var error = "Test error";

        // Act
        var result = new Result<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_ReturnsSuccessResult()
    {
        // Act
        var result = Result<string>.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_ReturnsFailureResult()
    {
        // Arrange
        var error = "Test error";

        // Act
        var result = Result<string>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitOperatorFromError_CreatesFailureResult()
    {
        // Arrange
        var error = "Test error";

        // Act
        Result<string> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    #endregion

    #region Result<TError> Match Tests

    [Fact]
    public void Match_OnSuccess_CallsSuccessFunc()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = result.Match(() => "success", _ => "failure");

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public void Match_OnFailure_CallsFailureFunc()
    {
        // Arrange
        var error = "Test error";
        var result = Result<string>.Failure(error);

        // Act
        var output = result.Match(() => "success", e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: Test error");
    }

    #endregion

    #region Result<TError> Bind Tests

    [Fact]
    public void Bind_OnSuccess_CallsBindFunc()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = result.Bind(() => Result<string>.Failure("bound error"));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("bound error");
    }

    [Fact]
    public void Bind_OnFailure_DoesNotCallBindFunc()
    {
        // Arrange
        var error = "original error";
        var result = Result<string>.Failure(error);
        var bindFuncCalled = false;

        // Act
        var output = result.Bind(() =>
        {
            bindFuncCalled = true;
            return Result<string>.Success();
        });

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(error);
        bindFuncCalled.Should().BeFalse();
    }

    [Fact]
    public void Bind_OnSuccess_PreservesSuccessFromBoundFunc()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = result.Bind(() => Result<string>.Success());

        // Assert
        output.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region Result<TError> MapError Tests

    [Fact]
    public void MapError_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = result.MapError(e => 42);

        // Assert
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void MapError_OnFailure_MapsError()
    {
        // Arrange
        var result = Result<string>.Failure("test error");

        // Act
        var output = result.MapError(e => e.Length);

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(10);
    }

    #endregion

    #region Result<TError> Tap Tests

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Result<string>.Success();
        var actionExecuted = false;

        // Act
        var output = result.Tap(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<string>.Failure("error");
        var actionExecuted = false;

        // Act
        var output = result.Tap(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotExecuteAction()
    {
        // Arrange
        var result = Result<string>.Success();
        var actionExecuted = false;

        // Act
        var output = result.TapError(_ => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void TapError_OnFailure_ExecutesAction()
    {
        // Arrange
        var error = "test error";
        var result = Result<string>.Failure(error);
        var capturedError = "";

        // Act
        var output = result.TapError(e => capturedError = e);

        // Assert
        capturedError.Should().Be(error);
        output.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Result<TError> Async Tests

    [Fact]
    public async Task MatchAsync_OnSuccess_CallsSuccessAsync()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            _ => Task.FromResult("failure"));

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task MatchAsync_OnFailure_CallsFailureAsync()
    {
        // Arrange
        var error = "Test error";
        var result = Result<string>.Failure(error);

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: Test error");
    }

    [Fact]
    public async Task MatchAsync_WithMixedAsync_OnSuccess_CallsSuccessAsync()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            _ => "failure");

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task MatchAsync_WithMixedAsync_OnFailure_CallsFailure()
    {
        // Arrange
        var error = "Test error";
        var result = Result<string>.Failure(error);

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: Test error");
    }

    [Fact]
    public async Task MatchAsync_SyncSuccess_AsyncFailure_OnSuccess()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = await result.MatchAsync(
            () => "success",
            _ => Task.FromResult("failure"));

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task MatchAsync_SyncSuccess_AsyncFailure_OnFailure()
    {
        // Arrange
        var error = "Test error";
        var result = Result<string>.Failure(error);

        // Act
        var output = await result.MatchAsync(
            () => "success",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: Test error");
    }

    [Fact]
    public async Task BindAsync_OnSuccess_CallsBindAsync()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = await result.BindAsync(() => Task.FromResult(Result<string>.Failure("bound error")));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("bound error");
    }

    [Fact]
    public async Task BindAsync_OnFailure_DoesNotCallBindAsync()
    {
        // Arrange
        var error = "original error";
        var result = Result<string>.Failure(error);
        var bindFuncCalled = false;

        // Act
        var output = await result.BindAsync(() =>
        {
            bindFuncCalled = true;
            return Task.FromResult(Result<string>.Success());
        });

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(error);
        bindFuncCalled.Should().BeFalse();
    }

    [Fact]
    public async Task MapErrorAsync_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = await result.MapErrorAsync(e => Task.FromResult(42));

        // Assert
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task MapErrorAsync_OnFailure_MapsErrorAsync()
    {
        // Arrange
        var result = Result<string>.Failure("test error");

        // Act
        var output = await result.MapErrorAsync(e => Task.FromResult(e.Length));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(10);
    }

    [Fact]
    public async Task TapAsync_OnSuccess_ExecutesActionAsync()
    {
        // Arrange
        var result = Result<string>.Success();
        var actionExecuted = false;

        // Act
        var output = await result.TapAsync(async () =>
        {
            await Task.Delay(0);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_OnFailure_DoesNotExecuteActionAsync()
    {
        // Arrange
        var result = Result<string>.Failure("error");
        var actionExecuted = false;

        // Act
        var output = await result.TapAsync(async () =>
        {
            await Task.Delay(0);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TapErrorAsync_OnSuccess_DoesNotExecuteActionAsync()
    {
        // Arrange
        var result = Result<string>.Success();
        var actionExecuted = false;

        // Act
        var output = await result.TapErrorAsync(async _ =>
        {
            await Task.Delay(0);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnFailure_ExecutesActionAsync()
    {
        // Arrange
        var error = "test error";
        var result = Result<string>.Failure(error);
        var capturedError = "";

        // Act
        var output = await result.TapErrorAsync(async e =>
        {
            await Task.Delay(0);
            capturedError = e;
        });

        // Assert
        capturedError.Should().Be(error);
        output.IsSuccess.Should().BeFalse();
    }

    #endregion
}

public class ValueResultTests
{
    #region ValueResult<TValue, TError> Constructor and Factory Tests

    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Act
        var result = ValueResult<string, int>.Success("test value");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("test value");
        result.Error.Should().Be(0);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Arrange
        var error = 42;

        // Act
        var result = ValueResult<string, int>.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
        result.Value.Should().BeNull();
    }

    [Fact]
    public void ImplicitOperatorFromValue_CreatesSuccessResult()
    {
        // Arrange
        var value = "test value";

        // Act
        ValueResult<string, int> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitOperatorFromError_CreatesFailureResult()
    {
        // Arrange
        var error = 42;

        // Act
        ValueResult<string, int> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    #endregion

    #region ValueResult<TValue, TError> Match Tests

    [Fact]
    public void Match_OnSuccess_CallsSuccessFunc()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test value");

        // Act
        var output = result.Match(v => $"success: {v}", e => $"failure: {e}");

        // Assert
        output.Should().Be("success: test value");
    }

    [Fact]
    public void Match_OnFailure_CallsFailureFunc()
    {
        // Arrange
        var error = 42;
        var result = ValueResult<string, int>.Failure(error);

        // Act
        var output = result.Match(v => $"success: {v}", e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: 42");
    }

    #endregion

    #region ValueResult<TValue, TError> Bind Tests

    [Fact]
    public void Bind_OnSuccess_CallsBindFunc()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = result.Bind<int>(v => ValueResult<int, int>.Success(v.Length));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public void Bind_OnSuccess_PropagatesFailure()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = result.Bind<int>(v => ValueResult<int, int>.Failure(99));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(99);
    }

    [Fact]
    public void Bind_OnFailure_DoesNotCallBindFunc()
    {
        // Arrange
        var error = 42;
        var result = ValueResult<string, int>.Failure(error);
        var bindFuncCalled = false;

        // Act
        var output = result.Bind<int>(v =>
        {
            bindFuncCalled = true;
            return ValueResult<int, int>.Success(0);
        });

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(error);
        bindFuncCalled.Should().BeFalse();
    }

    #endregion

    #region ValueResult<TValue, TError> Map Tests

    [Fact]
    public void Map_OnSuccess_MapsValue()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = result.Map<int>(v => v.Length);

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public void Map_OnFailure_DoesNotMapValue()
    {
        // Arrange
        var error = 42;
        var result = ValueResult<string, int>.Failure(error);

        // Act
        var output = result.Map<int>(v => v.Length);

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(error);
    }

    #endregion

    #region ValueResult<TValue, TError> MapError Tests

    [Fact]
    public void MapError_OnSuccess_ReturnsSuccess()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = result.MapError<string>(e => $"error: {e}");

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test");
    }

    [Fact]
    public void MapError_OnFailure_MapsError()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);

        // Act
        var output = result.MapError<string>(e => $"error: {e}");

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error: 42");
    }

    #endregion

    #region ValueResult<TValue, TError> Tap Tests

    [Fact]
    public void Tap_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");
        var capturedValue = "";

        // Act
        var output = result.Tap(v => capturedValue = v);

        // Assert
        capturedValue.Should().Be("test");
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Tap_OnFailure_DoesNotExecuteAction()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);
        var actionExecuted = false;

        // Act
        var output = result.Tap(v => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void TapError_OnSuccess_DoesNotExecuteAction()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");
        var actionExecuted = false;

        // Act
        var output = result.TapError(_ => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void TapError_OnFailure_ExecutesAction()
    {
        // Arrange
        var error = 42;
        var result = ValueResult<string, int>.Failure(error);
        var capturedError = 0;

        // Act
        var output = result.TapError(e => capturedError = e);

        // Assert
        capturedError.Should().Be(error);
        output.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ValueResult<TValue, TError> Async Tests

    [Fact]
    public async Task MatchAsync_OnSuccess_CallsSuccessAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task MatchAsync_OnFailure_CallsFailureAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task MatchAsync_WithMixedAsync_OnSuccess()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task MatchAsync_WithMixedAsync_OnFailure()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task MatchAsync_SyncSuccess_AsyncFailure_OnSuccess()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.MatchAsync(
            v => $"success: {v}",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task MatchAsync_SyncSuccess_AsyncFailure_OnFailure()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);

        // Act
        var output = await result.MatchAsync(
            v => $"success: {v}",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task BindAsync_OnSuccess_CallsBindAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.BindAsync<int>(v => Task.FromResult(ValueResult<int, int>.Success(v.Length)));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public async Task BindAsync_OnSuccess_PropagatesFailureAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.BindAsync<int>(v => Task.FromResult(ValueResult<int, int>.Failure(99)));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(99);
    }

    [Fact]
    public async Task BindAsync_OnFailure_DoesNotCallBindAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);
        var bindFuncCalled = false;

        // Act
        var output = await result.BindAsync<int>(v =>
        {
            bindFuncCalled = true;
            return Task.FromResult(ValueResult<int, int>.Success(0));
        });

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(42);
        bindFuncCalled.Should().BeFalse();
    }

    [Fact]
    public async Task MapAsync_OnSuccess_MapsValueAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.MapAsync<int>(v => Task.FromResult(v.Length));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public async Task MapAsync_OnFailure_DoesNotMapValueAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);

        // Act
        var output = await result.MapAsync<int>(v => Task.FromResult(v.Length));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(42);
    }

    [Fact]
    public async Task MapErrorAsync_OnSuccess_ReturnsSuccessAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");

        // Act
        var output = await result.MapErrorAsync<string>(e => Task.FromResult($"error: {e}"));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test");
    }

    [Fact]
    public async Task MapErrorAsync_OnFailure_MapsErrorAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);

        // Act
        var output = await result.MapErrorAsync<string>(e => Task.FromResult($"error: {e}"));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error: 42");
    }

    [Fact]
    public async Task TapAsync_OnSuccess_ExecutesActionAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");
        var capturedValue = "";

        // Act
        var output = await result.TapAsync(async v =>
        {
            await Task.Delay(0);
            capturedValue = v;
        });

        // Assert
        capturedValue.Should().Be("test");
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapAsync_OnFailure_DoesNotExecuteActionAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);
        var actionExecuted = false;

        // Act
        var output = await result.TapAsync(async v =>
        {
            await Task.Delay(0);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TapErrorAsync_OnSuccess_DoesNotExecuteActionAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Success("test");
        var actionExecuted = false;

        // Act
        var output = await result.TapErrorAsync(async e =>
        {
            await Task.Delay(0);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TapErrorAsync_OnFailure_ExecutesActionAsync()
    {
        // Arrange
        var result = ValueResult<string, int>.Failure(42);
        var capturedError = 0;

        // Act
        var output = await result.TapErrorAsync(async e =>
        {
            await Task.Delay(0);
            capturedError = e;
        });

        // Assert
        capturedError.Should().Be(42);
        output.IsSuccess.Should().BeFalse();
    }

    #endregion
}

public class ResultExtensionsTests
{
    #region Task<Result<TError>> Extension Tests

    [Fact]
    public async Task TaskResultMatchAsync_OnSuccess_CallsSuccessAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MatchAsync(() => "success", _ => "failure");

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task TaskResultMatchAsync_OnFailure_CallsFailureAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MatchAsync(() => "success", e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: error");
    }

    [Fact]
    public async Task TaskResultMatchAsync_AllAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            _ => Task.FromResult("failure"));

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task TaskResultMatchAsync_AllAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: error");
    }

    [Fact]
    public async Task TaskResultMatchAsync_AsyncSuccess_SyncFailuure_OnSuccessAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task TaskResultMatchAsync_AsyncSuccess_SyncFailuure_OnFailureSync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MatchAsync(
            () => Task.FromResult("success"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: error");
    }

    [Fact]
    public async Task TaskResultMatchAsync_SyncSuccess_AsyncFailure_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MatchAsync(
            () => "success",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("success");
    }

    [Fact]
    public async Task TaskResultMatchAsync_SyncSuccess_AsyncFailure_OnFailureAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MatchAsync(
            () => "success",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: error");
    }

    [Fact]
    public async Task TaskResultBindAsync_OnSuccess_CallsBindFunc()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.BindAsync(() => Result<string>.Failure("bound"));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("bound");
    }

    [Fact]
    public async Task TaskResultBindAsync_OnSuccess_CallsBindAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.BindAsync(() => Task.FromResult(Result<string>.Failure("bound")));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("bound");
    }

    [Fact]
    public async Task TaskResultMapErrorAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MapErrorAsync(e => e.Length);

        // Assert
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResultMapErrorAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MapErrorAsync(e => e.Length);

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(5);
    }

    [Fact]
    public async Task TaskResultMapErrorAsync_OnFailure_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MapErrorAsync(e => Task.FromResult(e.Length));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(5);
    }

    [Fact]
    public async Task TaskResultTapAsync_OnSuccess_ExecutesAction()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());
        var actionExecuted = false;

        // Act
        var output = await result.TapAsync(() => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResultTapAsync_OnSuccess_ExecutesActionAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());
        var actionExecuted = false;

        // Act
        var output = await result.TapAsync(async () =>
        {
            await Task.Delay(0);
            actionExecuted = true;
        });

        // Assert
        actionExecuted.Should().BeTrue();
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TaskResultTapErrorAsync_OnFailure_ExecutesAction()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));
        var capturedError = "";

        // Act
        var output = await result.TapErrorAsync(e => capturedError = e);

        // Assert
        capturedError.Should().Be("error");
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TaskResultTapErrorAsync_OnFailure_ExecutesActionAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));
        var capturedError = "";

        // Act
        var output = await result.TapErrorAsync(async e =>
        {
            await Task.Delay(0);
            capturedError = e;
        });

        // Assert
        output.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region Result Conversion Tests

    [Fact]
    public void MapToValueResult_WithValue_CreatesSuccessValueResult()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = result.MapToValueResult("test value");

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test value");
    }

    [Fact]
    public void MapToValueResult_WithValueFunc_CreatesSuccessValueResult()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = result.MapToValueResult(() => "test value");

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test value");
    }

    [Fact]
    public void MapToValueResult_OnFailure_WithValue_CreatesFailureValueResult()
    {
        // Arrange
        var result = Result<string>.Failure("error");

        // Act
        var output = result.MapToValueResult("test value");

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error");
    }

    [Fact]
    public void MapToValueResult_OnFailure_WithFunc_CreatesFailureValueResult()
    {
        // Arrange
        var result = Result<string>.Failure("error");

        // Act
        var output = result.MapToValueResult(() => "test value");

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error");
    }

    [Fact]
    public async Task MapToValueResultAsync_OnSuccess_WithAsync_CreatesSuccessValueResult()
    {
        // Arrange
        var result = Result<string>.Success();

        // Act
        var output = await result.MapToValueResultAsync(() => Task.FromResult("test value"));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test value");
    }

    [Fact]
    public async Task MapToValueResultAsync_OnFailure_WithAsync_CreatesFailureValueResult()
    {
        // Arrange
        var result = Result<string>.Failure("error");

        // Act
        var output = await result.MapToValueResultAsync(() => Task.FromResult("test value"));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error");
    }

    [Fact]
    public async Task TaskResultMapToValueResultAsync_OnSuccess_WithValue()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MapToValueResultAsync("test value");

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test value");
    }

    [Fact]
    public async Task TaskResultMapToValueResultAsync_OnSuccess_WithFunc()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MapToValueResultAsync(() => "test value");

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test value");
    }

    [Fact]
    public async Task TaskResultMapToValueResultAsync_OnSuccess_WithFuncAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Success());

        // Act
        var output = await result.MapToValueResultAsync(() => Task.FromResult("test value"));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test value");
    }

    [Fact]
    public async Task TaskResultMapToValueResultAsync_OnFailure_WithValue()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MapToValueResultAsync("test value");

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error");
    }

    [Fact]
    public async Task TaskResultMapToValueResultAsync_OnFailure_WithFunc()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MapToValueResultAsync(() => "test value");

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error");
    }

    [Fact]
    public async Task TaskResultMapToValueResultAsync_OnFailure_WithFuncAsync()
    {
        // Arrange
        var result = Task.FromResult(Result<string>.Failure("error"));

        // Act
        var output = await result.MapToValueResultAsync(() => Task.FromResult("test value"));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error");
    }

    #endregion

    #region Task<ValueResult<TValue, TError>> Extension Tests

    [Fact]
    public async Task TaskValueResultMatchAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MatchAsync(v => $"success: {v}", e => $"failure: {e}");

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MatchAsync(v => $"success: {v}", e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_AllAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_AllAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_SuccessSync_FailureAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MatchAsync(
            v => $"success: {v}",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_SuccessSync_FailureAsync_OnFailureAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MatchAsync(
            v => $"success: {v}",
            e => Task.FromResult($"failure: {e}"));

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_SuccessAsync_FailureSync_OnSuccessAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("success: test");
    }

    [Fact]
    public async Task TaskValueResultMatchAsync_SuccessAsync_FailureSync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MatchAsync(
            v => Task.FromResult($"success: {v}"),
            e => $"failure: {e}");

        // Assert
        output.Should().Be("failure: 42");
    }

    [Fact]
    public async Task TaskValueResultBindAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.BindAsync(v => ValueResult<int, int>.Success(v.Length));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public async Task TaskValueResultBindAsync_OnSuccess_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.BindAsync(v => Task.FromResult(ValueResult<int, int>.Success(v.Length)));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public async Task TaskValueResultBindAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));
        var bindFuncCalled = false;

        // Act
        var output = await result.BindAsync(v =>
        {
            bindFuncCalled = true;
            return ValueResult<int, int>.Success(0);
        });

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(42);
        bindFuncCalled.Should().BeFalse();
    }

    [Fact]
    public async Task TaskValueResultMapAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MapAsync(v => v.Length);

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public async Task TaskValueResultMapAsync_OnSuccess_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MapAsync(v => Task.FromResult(v.Length));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be(4);
    }

    [Fact]
    public async Task TaskValueResultMapAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MapAsync(v => v.Length);

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be(42);
    }

    [Fact]
    public async Task TaskValueResultMapErrorAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MapErrorAsync(e => $"error: {e}");

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test");
    }

    [Fact]
    public async Task TaskValueResultMapErrorAsync_OnSuccess_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));

        // Act
        var output = await result.MapErrorAsync(e => Task.FromResult($"error: {e}"));

        // Assert
        output.IsSuccess.Should().BeTrue();
        output.Value.Should().Be("test");
    }

    [Fact]
    public async Task TaskValueResultMapErrorAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MapErrorAsync(e => $"error: {e}");

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error: 42");
    }

    [Fact]
    public async Task TaskValueResultMapErrorAsync_OnFailure_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));

        // Act
        var output = await result.MapErrorAsync(e => Task.FromResult($"error: {e}"));

        // Assert
        output.IsSuccess.Should().BeFalse();
        output.Error.Should().Be("error: 42");
    }

    [Fact]
    public async Task TaskValueResultTapAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));
        var capturedValue = "";

        // Act
        var output = await result.TapAsync(v => capturedValue = v);

        // Assert
        capturedValue.Should().Be("test");
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TaskValueResultTapAsync_OnSuccess_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));
        var capturedValue = "";

        // Act
        var output = await result.TapAsync(async v =>
        {
            await Task.Delay(0);
            capturedValue = v;
        });

        // Assert
        capturedValue.Should().Be("test");
        output.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task TaskValueResultTapAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));
        var actionExecuted = false;

        // Act
        var output = await result.TapAsync(v => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TaskValueResultTapErrorAsync_OnFailure()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));
        var capturedError = 0;

        // Act
        var output = await result.TapErrorAsync(e => capturedError = e);

        // Assert
        capturedError.Should().Be(42);
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TaskValueResultTapErrorAsync_OnFailure_WithAsync()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Failure(42));
        var capturedError = 0;

        // Act
        var output = await result.TapErrorAsync(async e =>
        {
            await Task.Delay(0);
            capturedError = e;
        });

        // Assert
        capturedError.Should().Be(42);
        output.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task TaskValueResultTapErrorAsync_OnSuccess()
    {
        // Arrange
        var result = Task.FromResult(ValueResult<string, int>.Success("test"));
        var actionExecuted = false;

        // Act
        var output = await result.TapErrorAsync(e => actionExecuted = true);

        // Assert
        actionExecuted.Should().BeFalse();
        output.IsSuccess.Should().BeTrue();
    }

    #endregion
}