using MediatR;
using Rack.Doamin.Commons.Primitives;

namespace Rack.Application.Primitives;

public interface IQuery<T> : IRequest<Response<T>>
{
}