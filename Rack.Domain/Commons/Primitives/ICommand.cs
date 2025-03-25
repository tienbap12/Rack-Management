using MediatR;

namespace Rack.Application.Wrappers
{
    public interface ICommand<out T> : IRequest<T>
    {
    }
}
