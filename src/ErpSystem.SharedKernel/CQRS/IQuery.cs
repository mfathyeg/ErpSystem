using ErpSystem.SharedKernel.Results;
using MediatR;

namespace ErpSystem.SharedKernel.CQRS;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

public interface ICachedQuery<TResponse> : IQuery<TResponse>
{
    string CacheKey { get; }
    TimeSpan? CacheDuration { get; }
}
