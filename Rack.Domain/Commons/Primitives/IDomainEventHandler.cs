using MediatR;

namespace Rack.Application.Primitives;

public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
{

}