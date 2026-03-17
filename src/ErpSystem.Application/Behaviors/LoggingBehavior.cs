using System.Diagnostics;
using ErpSystem.SharedKernel.CQRS;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpSystem.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var isCommand = request is ICommand || request.GetType().GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        var requestType = isCommand ? "Command" : "Query";

        _logger.LogInformation(
            "Processing {RequestType} {RequestName}",
            requestType,
            requestName);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed {RequestType} {RequestName} in {ElapsedMilliseconds}ms",
                requestType,
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error processing {RequestType} {RequestName} after {ElapsedMilliseconds}ms",
                requestType,
                requestName,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
