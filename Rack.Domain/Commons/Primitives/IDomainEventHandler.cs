using MediatR;

namespace Rack.Application.Wrappers
{
    public interface IDomainEventHandler<in TDomainEvent> : INotificationHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {

    }
}