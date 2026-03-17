using ErpSystem.Application.Abstractions.Messaging;
using ErpSystem.SharedKernel.Domain;
using MassTransit;

namespace ErpSystem.Infrastructure.Messaging;

public sealed class MassTransitEventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;

    public MassTransitEventBus(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        await _publishEndpoint.Publish(@event, cancellationToken);
    }
}
