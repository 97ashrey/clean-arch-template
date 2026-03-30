namespace Company.Service.Application.Common.Types;

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
}