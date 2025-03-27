using MediatR;
using Rack.Doamin.Commons.Primitives;

namespace Rack.Application.Primitives;

public interface IQueryHandler<in TIn, TOut> : IRequestHandler<TIn, Response<TOut>>
    where TIn : IQuery<TOut>
{
}