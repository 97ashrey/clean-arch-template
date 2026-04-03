namespace Company.Service.Domain.Common.Types;

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
        => this.IsSuccess ? bind(this.value!) : (Result<TResult, TError>)this.error!;

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
}

public static class ResultExtensions
{
    extension<TError>(Result<TError> result)
    {
        public Result<TValue, TError> Map<TValue>(Func<TValue> map)
            => result.IsSuccess ? map() : result.Error!;
    }

    extension<TValue, TError>(Result<TValue, TError> result)
    {
        public Result<TError> Bind<TValueOther>(Func<TValue, Result<TError>> bind)
            => result.IsSuccess ? bind(result.Value!) : result.Error!;
    }
}