using Company.Service.Application.Common.Types;
using Company.Service.Application.Common.Types.Errors;
using Mediator;

namespace Company.Service.Application.Common.Requests;

internal interface IApplicationRequest<TResponse> : IRequest<TResponse>
{
    TResponse CreateApplicationErrorResponse(ApplicationError error);

    ApplicationError? ExtractError(TResponse response);
}

public abstract record class ApplicationRequest<TResponse> : IApplicationRequest<Result<TResponse, ApplicationError>>
{
    public Result<TResponse, ApplicationError> CreateApplicationErrorResponse(ApplicationError error)
    {
        return error;
    }

    public ApplicationError? ExtractError(Result<TResponse, ApplicationError> response)
    {
        return response.Error;
    }
}

internal interface IApplicationRequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, Result<TResponse, ApplicationError>> where TRequest : IRequest<Result<TResponse, ApplicationError>> { }