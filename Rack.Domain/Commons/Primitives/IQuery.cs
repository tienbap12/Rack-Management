using MediatR;
using Rack.Doamin.Commons.Primitives;

namespace Rack.Application.Wrappers
{
    public interface IQuery<T> : IRequest<Response<T>>
    {
    }
}