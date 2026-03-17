using ErpSystem.SharedKernel.Results;
using MediatR;

namespace ErpSystem.SharedKernel.CQRS;

public interface ICommand : IRequest<Result>
{
    Guid CommandId { get; }
}

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
    Guid CommandId { get; }
}

public abstract record Command : ICommand
{
    public Guid CommandId { get; init; } = Guid.NewGuid();
}

public abstract record Command<TResponse> : ICommand<TResponse>
{
    public Guid CommandId { get; init; } = Guid.NewGuid();
}
