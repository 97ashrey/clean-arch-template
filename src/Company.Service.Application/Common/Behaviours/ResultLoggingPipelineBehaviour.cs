using Company.Service.Application.Common.Requests;
using Mediator;
using Microsoft.Extensions.Logging;

namespace Company.Service.Application.Common.Behaviours;

internal class ResultLoggingPipelineBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IApplicationRequest<TResponse>
{
    private readonly ILogger<ResultLoggingPipelineBehaviour<TRequest, TResponse>> _logger;

    public ResultLoggingPipelineBehaviour(ILogger<ResultLoggingPipelineBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async ValueTask<TResponse> Handle(TRequest message, MessageHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        var result = await next(message, cancellationToken);

        var error = message.ExtractError(result);

        if (error is not null)
        {
            _logger.LogError("Application responded with an error: {error}", error.ToString());
        }

        return result;
    }
}