using MediatR;
using Rack.Domain.Commons.Primitives;

namespace Rack.Domain.Primitives;

public interface IQueryHandler<in TIn, TOut> : IRequestHandler<TIn, Response<TOut>>
    where TIn : IQuery<TOut>
{
}