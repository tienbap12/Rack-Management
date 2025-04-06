using MediatR;
using Rack.Domain.Commons.Primitives;

namespace Rack.Domain.Primitives;

public interface IQuery<T> : IRequest<Response<T>>
{
}