using MediatR;

namespace Rack.Application.Primitives;

public interface ICommandHandler<in TCommand, T> : IRequestHandler<TCommand, T>
    where TCommand : ICommand<T>
{
}