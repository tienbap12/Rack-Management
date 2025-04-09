using MediatR;

namespace Rack.Domain.Commons.Primitives;

public interface ICommandHandler<in TCommand, T> : IRequestHandler<TCommand, T>
    where TCommand : ICommand<T>
{
}