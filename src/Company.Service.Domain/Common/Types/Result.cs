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

    private Result(TError error)
    {
        this.error = error;
        this.IsSuccess = false;
    }

    public static implicit operator Result<TError>(TError error) => new(error);

    public TResult Match<TResult>(
            Func<TResult> success,
            Func<TError, TResult> failure
        ) => this.IsSuccess ? success() : failure(this.error!);

    public Result<TError> Bind(Func<Result<TError>> bind)
        => this.IsSuccess ? bind() : this;

    public Result<NewError> MapError<NewError>(Func<TError, NewError> map)
        => this.IsSuccess ? new Result<NewError>() : map(this.Error!);

    public Result<TError> Tap(Action action)
    {
        if (this.IsSuccess)
            action();

        return this;
    }

    public Result<TError> TapError(Action<TError> action)
    {
        if (!this.IsSuccess)
            action(this.error!);

        return this;
    }

    public async Task<TResult> MatchAsync<TResult>(
            Func<Task<TResult>> success,
            Func<TError, Task<TResult>> failure
        ) => this.IsSuccess ? await success() : await failure(this.error!);

    public async Task<Result<TError>> BindAsync(Func<Task<Result<TError>>> bind)
        => this.IsSuccess ? await bind() : this;

    public async Task<Result<NewError>> MapErrorAsync<NewError>(Func<TError, Task<NewError>> map)
        => this.IsSuccess ? new Result<NewError>() : await map(this.Error!);

    public async Task<Result<TError>> TapAsync(Func<Task> action)
    {
        if (this.IsSuccess)
            await action();

        return this;
    }

    public async Task<Result<TError>> TapErrorAsync(Func<TError, Task> action)
    {
        if (!this.IsSuccess)
            await action(this.error!);

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

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TResult Match<TResult>(
            Func<TValue, TResult> success,
            Func<TError, TResult> failure
        ) => this.IsSuccess ? success(this.value!) : failure(this.error!);

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
            Func<TValue, Task<TResult>> success,
            Func<TError, Task<TResult>> failure
        ) => this.IsSuccess ? await success(this.value!) : await failure(this.error!);

    public async Task<Result<TResult, TError>> BindAsync<TResult>(Func<TValue, Task<Result<TResult, TError>>> bind)
        => this.IsSuccess ? await bind(this.value!) : (Result<TResult, TError>)this.error!;

    public async Task<Result<TValueOther, TError>> MapAsync<TValueOther>(Func<TValue, Task<TValueOther>> map)
        => this.IsSuccess ? await map(this.value!) : this.error!;

    public async Task<Result<TValue, TErrorOther>> MapErrorAsync<TErrorOther>(Func<TError, Task<TErrorOther>> map)
        => this.IsSuccess ? this.value! : await map(this.error!);

    public async Task<Result<TValue, TError>> TapAsync(Func<TValue, Task> action)
    {
        if (this.IsSuccess)
            await action(this.value!);

        return this;
    }

    public async Task<Result<TValue, TError>> TapErrorAsync(Func<TError, Task> action)
    {
        if (!this.IsSuccess)
            await action(this.error!);

        return this;
    }
}

public static class ResultExtensions
{
    extension<TError>(Result<TError> result)
    {
        public Result<TValue, TError> MapToValueResult<TValue>(Func<TValue> map)
            => result.IsSuccess ? map() : result.Error!;

        public Result<TValue, TError> MapToValueResult<TValue>(TValue value)
            => result.IsSuccess ? value : result.Error!;

        public async Task<Result<TValue, TError>> MapToValueResultAsync<TValue>(Func<Task<TValue>> map)
            => result.IsSuccess ? await map() : result.Error!;
    }

    extension<TValue, TError>(Result<TValue, TError> result)
    {
        public Result<TError> Bind<TValueOther>(Func<TValue, Result<TError>> bind)
            => result.IsSuccess ? bind(result.Value!) : result.Error!;

        public async Task<Result<TError>> BindAsync<TValueOther>(Func<TValue, Task<Result<TError>>> bind)
            => result.IsSuccess ? await bind(result.Value!) : result.Error!;
    }
}