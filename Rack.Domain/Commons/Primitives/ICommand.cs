using MediatR;

namespace Rack.Domain.Commons.Primitives;

public interface ICommand<out T> : IRequest<T>
{
}