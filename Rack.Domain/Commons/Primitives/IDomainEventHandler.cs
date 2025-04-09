using MediatR;

namespace Rack.Domain.Commons.Primitives;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{
}