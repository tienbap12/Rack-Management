using MediatR;
using Rack.Doamin.Commons.Primitives;

namespace Rack.Application.Wrappers
{
    public interface IQueryHandler<in TIn, TOut> : IRequestHandler<TIn, Response<TOut>>
        where TIn : IQuery<TOut>
    {
    }
}