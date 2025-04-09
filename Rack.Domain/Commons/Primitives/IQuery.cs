using MediatR;

namespace Rack.Domain.Commons.Primitives;

public interface IQuery<T> : IRequest<Response<T>>
{
}