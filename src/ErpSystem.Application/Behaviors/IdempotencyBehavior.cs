using ErpSystem.Application.Abstractions.Idempotency;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpSystem.Application.Behaviors;

public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
    where TResponse : Result
{
    private readonly IIdempotencyService _idempotencyService;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;

    public IdempotencyBehavior(
        IIdempotencyService idempotencyService,
        ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _idempotencyService = idempotencyService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var commandId = request.CommandId;

        if (await _idempotencyService.IsProcessedAsync(commandId, cancellationToken))
        {
            _logger.LogWarning(
                "Command {CommandName} with ID {CommandId} was already processed",
                typeof(TRequest).Name,
                commandId);

            return CreateIdempotentResult<TResponse>();
        }

        var response = await next();

        if (response.IsSuccess)
        {
            await _idempotencyService.MarkAsProcessedAsync(commandId, cancellationToken);
        }

        return response;
    }

    private static TResult CreateIdempotentResult<TResult>() where TResult : Result
    {
        if (typeof(TResult) == typeof(Result))
        {
            return (Result.Success() as TResult)!;
        }

        var resultType = typeof(TResult).GetGenericArguments()[0];
        var defaultValue = resultType.IsValueType ? Activator.CreateInstance(resultType) : null;

        var successMethod = typeof(Result)
            .GetMethod(nameof(Result.Success), 1, Type.EmptyTypes)!
            .MakeGenericMethod(resultType);

        return (TResult)successMethod.Invoke(null, null)!;
    }
}
