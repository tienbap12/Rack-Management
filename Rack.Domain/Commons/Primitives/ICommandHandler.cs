using MediatR;

namespace Rack.Domain.Primitives;

public interface ICommandHandler<in TCommand, T> : IRequestHandler<TCommand, T>
    where TCommand : ICommand<T>
{
}