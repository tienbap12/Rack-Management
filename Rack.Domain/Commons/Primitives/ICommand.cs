using MediatR;

namespace Rack.Domain.Primitives;

public interface ICommand<out T> : IRequest<T>
{
}