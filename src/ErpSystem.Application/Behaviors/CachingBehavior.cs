using ErpSystem.Application.Abstractions.Caching;
using ErpSystem.SharedKernel.CQRS;
using ErpSystem.SharedKernel.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ErpSystem.Application.Behaviors;

public sealed class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICachedQuery<TResponse>
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cacheService,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var cacheKey = request.CacheKey;

        var cachedResult = await _cacheService.GetAsync<TResponse>(cacheKey, cancellationToken);
        if (cachedResult is not null)
        {
            _logger.LogDebug("Cache hit for key {CacheKey}", cacheKey);
            return cachedResult;
        }

        _logger.LogDebug("Cache miss for key {CacheKey}", cacheKey);

        var response = await next();

        if (response is Result { IsSuccess: true })
        {
            var duration = request.CacheDuration ?? TimeSpan.FromMinutes(5);
            await _cacheService.SetAsync(cacheKey, response, duration, cancellationToken);
        }

        return response;
    }
}
