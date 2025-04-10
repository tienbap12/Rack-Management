using Mapster;
using Rack.Contracts.ServerRental.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ServerRental.Queries.GetById
{
    public class GetServerRentalByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetServerRentalByIdQuery, ServerRentalResponse>
    {
        public async Task<Response<ServerRentalResponse>> Handle(GetServerRentalByIdQuery request, CancellationToken cancellationToken)
        {
            var serverRentalRepo = unitOfWork.GetRepository<Domain.Entities.ServerRental>();
            var rental = await serverRentalRepo.GetByIdAsync(request.RentalId, cancellationToken);

            if (rental == null)
            {
                return Response<ServerRentalResponse>.Failure(Error.NotFound("Server rental not found"));
            }

            var result = rental.Adapt<ServerRentalResponse>();
            return Response<ServerRentalResponse>.Success(result);
        }
    }
}