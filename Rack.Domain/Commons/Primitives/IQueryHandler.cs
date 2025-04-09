using MediatR;

namespace Rack.Domain.Commons.Primitives;

public interface IQueryHandler<in TIn, TOut> : IRequestHandler<TIn, Response<TOut>>
    where TIn : IQuery<TOut>
{
}