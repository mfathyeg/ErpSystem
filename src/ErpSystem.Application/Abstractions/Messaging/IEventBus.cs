using ErpSystem.SharedKernel.Domain;

namespace ErpSystem.Application.Abstractions.Messaging;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;
}
