using MediatR;
using Rack.Domain.Data;

namespace Rack.Application.Commons.Behaviors{
    public class UnitOfWorkBehavior<TTRequest, TTResponse>(IUnitOfWork unitOfWork)
        : IPipelineBehavior<TTRequest, TTResponse>
        where TTRequest : notnull
        where TTResponse : notnull{
        public async Task<TTResponse> Handle(TTRequest request,
            RequestHandlerDelegate<TTResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();

            try
            {
                await unitOfWork.CommitAsync(cancellationToken);
            }
            catch
            {
                await unitOfWork.RollBackAsync(cancellationToken);
            }

            return response;
        }
    }
}