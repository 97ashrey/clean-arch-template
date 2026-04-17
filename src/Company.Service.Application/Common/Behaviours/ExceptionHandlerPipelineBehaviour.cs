using Company.Service.Application.Common.Requests;
using Company.Service.Application.Common.Types.Errors;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Company.Service.Application.Common.Behaviours;

internal class ExceptionHandlerPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IApplicationRequest<TResponse>
{
    private readonly ILogger<ExceptionHandlerPipelineBehaviour<TRequest, TResponse>> _logger;

    public ExceptionHandlerPipelineBehaviour(ILogger<ExceptionHandlerPipelineBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(message, cancellationToken);
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid();

            using (_logger.BeginScope(new Dictionary<string, object>() { { "ErrorId", errorId.ToString() } }))
            {
                _logger.LogError(ex, "An unexpected error has occured.");

                var error = new ApplicationError() { Message = "An unexpected error has occured.", Id = errorId };

                return message.CreateApplicationErrorResponse(error);
            }
        }
    }
}