namespace Company.Service.Domain.Common.Types;

using System.Threading.Tasks;

public readonly struct Result<TError>
{
    private readonly TError? error;

    public TError? Error => error;

    public bool IsSuccess { get; }

    public Result()
    {
        this.error = default;
        this.IsSuccess = true;
    }

    public Result(TError error)
    {
        this.error = error;
        this.IsSuccess = false;
    }

    public static Result<TError> Success() => new();
    public static Result<TError> Failure(TError error) => new(error);

    public static implicit operator Result<TError>(TError error) => new(error);

    public TResult Match<TResult>(Func<TResult> success, Func<TError, TResult> failure)
        => this.IsSuccess ? success() : failure(this.error!);

    public Result<TError> Bind(Func<Result<TError>> bind)
        => this.IsSuccess ? bind() : this;

    public Result<NewError> MapError<NewError>(Func<TError, NewError> map)
        => this.IsSuccess ? new Result<NewError>() : map(this.Error!);

    public Result<TError> Tap(Action action)
    {
        if (this.IsSuccess)
        {
            action();
        }

        return this;
    }

    public Result<TError> TapError(Action<TError> action)
    {
        if (!this.IsSuccess)
        {
            action(this.error!);
        }

        return this;
    }

    public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> successAsync, Func<TError, Task<TResult>> failureAsync)
        => this.IsSuccess ? await successAsync() : await failureAsync(this.error!);

    public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> successAsync, Func<TError, TResult> failure)
        => this.IsSuccess ? await successAsync() : failure(this.error!);

    public async Task<TResult> MatchAsync<TResult>(Func<TResult> success, Func<TError, Task<TResult>> failureAsync)
        => this.IsSuccess ? success() : await failureAsync(this.error!);

    public async Task<Result<TError>> BindAsync(Func<Task<Result<TError>>> bindAsync)
        => this.IsSuccess ? await bindAsync() : this;

    public async Task<Result<NewError>> MapErrorAsync<NewError>(Func<TError, Task<NewError>> mapAsync)
        => this.IsSuccess ? new Result<NewError>() : await mapAsync(this.Error!);

    public async Task<Result<TError>> TapAsync(Func<Task> actionAsync)
    {
        if (this.IsSuccess)
        {
            await actionAsync();
        }

        return this;
    }

    public async Task<Result<TError>> TapErrorAsync(Func<TError, Task> actionAsync)
    {
        if (!this.IsSuccess)
        {
            await actionAsync(this.error!);
        }

        return this;
    }
}

public readonly struct Result<TValue, TError>
{
    private readonly TValue? value;
    private readonly TError? error;

    public TValue? Value => value;
    public TError? Error => error;

    public bool IsSuccess { get; }

    private Result(TValue value)
    {
        this.value = value;
        this.error = default;
        this.IsSuccess = true;
    }

    private Result(TError error)
    {
        this.value = default;
        this.error = error;
        this.IsSuccess = false;
    }

    public static Result<TValue, TError> Success(TValue value) => new(value);
    public static Result<TValue, TError> Failure(TError error) => new(error);

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
        => this.IsSuccess ? success(this.value!) : failure(this.error!);

    public Result<TResult, TError> Bind<TResult>(Func<TValue, Result<TResult, TError>> bind)
        => this.IsSuccess ? bind(this.value!) : this.error!;

    public Result<TValueOther, TError> Map<TValueOther>(Func<TValue, TValueOther> map)
        => this.IsSuccess ? map(this.value!) : this.error!;

    public Result<TValue, TErrorOther> MapError<TErrorOther>(Func<TError, TErrorOther> map)
        => this.IsSuccess ? this.value! : map(this.error!);

    public Result<TValue, TError> Tap(Action<TValue> action)
    {
        if (this.IsSuccess)
            action(this.value!);

        return this;
    }

    public Result<TValue, TError> TapError(Action<TError> action)
    {
        if (!this.IsSuccess)
            action(this.error!);

        return this;
    }

    public async Task<TResult> MatchAsync<TResult>(
            Func<TValue, Task<TResult>> successAsync,
            Func<TError, Task<TResult>> failureAsync
        ) => this.IsSuccess ? await successAsync(this.value!) : await failureAsync(this.error!);

    public async Task<TResult> MatchAsync<TResult>(
            Func<TValue, Task<TResult>> successAsync,
            Func<TError, TResult> failure
        ) => this.IsSuccess ? await successAsync(this.value!) : failure(this.error!);

    public async Task<TResult> MatchAsync<TResult>(
            Func<TValue, TResult> success,
            Func<TError, Task<TResult>> failureAsync
        ) => this.IsSuccess ? success(this.value!) : await failureAsync(this.error!);

    public async Task<Result<TResult, TError>> BindAsync<TResult>(Func<TValue, Task<Result<TResult, TError>>> bindAsync)
        => this.IsSuccess ? await bindAsync(this.value!) : (Result<TResult, TError>)this.error!;

    public async Task<Result<TValueOther, TError>> MapAsync<TValueOther>(Func<TValue, Task<TValueOther>> mapAsync)
        => this.IsSuccess ? await mapAsync(this.value!) : this.error!;

    public async Task<Result<TValue, TErrorOther>> MapErrorAsync<TErrorOther>(Func<TError, Task<TErrorOther>> mapAsync)
        => this.IsSuccess ? this.value! : await mapAsync(this.error!);

    public async Task<Result<TValue, TError>> TapAsync(Func<TValue, Task> actionAsync)
    {
        if (this.IsSuccess)
            await actionAsync(this.value!);

        return this;
    }

    public async Task<Result<TValue, TError>> TapErrorAsync(Func<TError, Task> actionAsync)
    {
        if (!this.IsSuccess)
            await actionAsync(this.error!);

        return this;
    }
}

public static class ResultExtensions
{
    extension<TError>(Result<TError> result)
    {
        public Result<TValue, TError> MapToValueResult<TValue>(TValue value)
            => result.IsSuccess ? value : result.Error!;

        public Result<TValue, TError> MapToValueResult<TValue>(Func<TValue> map)
            => result.IsSuccess ? map() : result.Error!;

        public async Task<Result<TValue, TError>> MapToValueResultAsync<TValue>(Func<Task<TValue>> mapAsync)
            => result.IsSuccess ? await mapAsync() : result.Error!;
    }

    extension<TError>(Task<Result<TError>> resultTask)
    {
        public async Task<Result<TValue, TError>> MapToValueResultAsync<TValue>(TValue value)
            => (await resultTask).MapToValueResult(value);

        public async Task<Result<TValue, TError>> MapToValueResultAsync<TValue>(Func<TValue> map)
            => (await resultTask).MapToValueResult(map);

        public async Task<Result<TValue, TError>> MapToValueResultAsync<TValue>(Func<Task<TValue>> mapAsync)
            => await (await resultTask).MapToValueResultAsync(mapAsync);

        public async Task<TResult> MatchAsync<TResult>(Func<TResult> success, Func<TError, TResult> failure)
            => (await resultTask).Match(success, failure);

        public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> successAsync, Func<TError, Task<TResult>> failureAsync)
            => await (await resultTask).MatchAsync(successAsync, failureAsync);

        public async Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> successAsync, Func<TError, TResult> failure)
            => await (await resultTask).MatchAsync(successAsync, failure);

        public async Task<TResult> MatchAsync<TResult>(Func<TResult> success, Func<TError, Task<TResult>> failureAsync)
            => await (await resultTask).MatchAsync(success, failureAsync);

        public async Task<Result<TError>> BindAsync(Func<Result<TError>> bind)
            => (await resultTask).Bind(bind);

        public async Task<Result<TError>> BindAsync(Func<Task<Result<TError>>> bindAsync)
            => await (await resultTask).BindAsync(bindAsync);

        public async Task<Result<NewError>> MapErrorAsync<NewError>(Func<TError, NewError> map)
            => (await resultTask).MapError(map);

        public async Task<Result<NewError>> MapErrorAsync<NewError>(Func<TError, Task<NewError>> mapAsync)
            => await (await resultTask).MapErrorAsync(mapAsync);

        public async Task<Result<TError>> TapAsync(Action action)
            => (await resultTask).Tap(action);

        public async Task<Result<TError>> TapAsync(Func<Task> actionAsync)
            => await (await resultTask).TapAsync(actionAsync);

        public async Task<Result<TError>> TapErrorAsync(Action<TError> action)
            => (await resultTask).TapError(action);

        public async Task<Result<TError>> TapErrorAsync(Func<TError, Task> actionAsync)
            => await (await resultTask).TapErrorAsync(actionAsync);
    }

    extension<TValue, TError>(Result<TValue, TError> result)
    {
        public Result<TError> MapToResult<TValueOther>()
            => result.IsSuccess ? new Result<TError>() : result.Error!;
    }

    extension<TValue, TError>(Task<Result<TValue, TError>> resultTask)
    {
        public async Task<TResult> MatchAsync<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
            => (await resultTask).Match(success, failure);

        public async Task<TResult> MatchAsync<TResult>(Func<TValue, Task<TResult>> successAsync, Func<TError, Task<TResult>> failureAsync)
            => await (await resultTask).MatchAsync(successAsync, failureAsync);

        public async Task<TResult> MatchAsync<TResult>(Func<TValue, Task<TResult>> successAsync, Func<TError, TResult> failure)
            => await (await resultTask).MatchAsync(successAsync, failure);

        public async Task<TResult> MatchAsync<TResult>(Func<TValue, TResult> success, Func<TError, Task<TResult>> failureAsync)
            => await (await resultTask).MatchAsync(success, failureAsync);

        public async Task<Result<TResult, TError>> BindAsync<TResult>(Func<TValue, Result<TResult, TError>> bind)
            => (await resultTask).Bind(bind);

        public async Task<Result<TResult, TError>> BindAsync<TResult>(Func<TValue, Task<Result<TResult, TError>>> bindAsync)
            => await (await resultTask).BindAsync(bindAsync);

        public async Task<Result<TValueOther, TError>> MapAsync<TValueOther>(Func<TValue, TValueOther> map)
            => (await resultTask).Map(map);

        public async Task<Result<TValueOther, TError>> MapAsync<TValueOther>(Func<TValue, Task<TValueOther>> mapAsync)
            => await (await resultTask).MapAsync(mapAsync);

        public async Task<Result<TValue, TErrorOther>> MapErrorAsync<TErrorOther>(Func<TError, TErrorOther> map)
            => (await resultTask).MapError(map);

        public async Task<Result<TValue, TErrorOther>> MapErrorAsync<TErrorOther>(Func<TError, Task<TErrorOther>> mapAsync)
            => await (await resultTask).MapErrorAsync(mapAsync);

        public async Task<Result<TValue, TError>> TapAsync(Action<TValue> action)
            => (await resultTask).Tap(action);

        public async Task<Result<TValue, TError>> TapAsync(Func<TValue, Task> actionAsync)
            => await (await resultTask).TapAsync(actionAsync);

        public async Task<Result<TValue, TError>> TapErrorAsync(Action<TError> action)
            => (await resultTask).TapError(action);

        public async Task<Result<TValue, TError>> TapErrorAsync(Func<TError, Task> actionAsync)
            => await (await resultTask).TapErrorAsync(actionAsync);

    }
}