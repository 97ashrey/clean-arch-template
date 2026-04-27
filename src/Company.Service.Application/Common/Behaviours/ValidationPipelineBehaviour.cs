using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using FluentValidation;
using Mediator;

namespace Company.Service.Application.Common.Behaviours;

internal class ValidationPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IApplicationRequest<TResponse>
{
    private readonly IValidator<TRequest>? _validator;

    public ValidationPipelineBehaviour(IValidator<TRequest>? validator = null)
    {
        _validator = validator;
    }

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        if (_validator is not null)
        {
            var res = _validator.Validate(message);

            if (!res.IsValid)
            {
                return message.CreateApplicationErrorResponse(new ValidationError()
                {
                    Message = "Validation failed.",
                    Failures = [.. res.ToDictionary().Select(kvp => new ValidationFailure(kvp.Key, kvp.Value))]
                });
            }
        }

        return await next(message, cancellationToken);
    }
}