using MediatR;

namespace Rack.Application.Primitives;

public interface ICommand<out T> : IRequest<T>
{
}
