using ErpSystem.SharedKernel.Results;
using MediatR;

namespace ErpSystem.SharedKernel.CQRS;

public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
