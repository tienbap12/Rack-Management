using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ServerRental.Commands.Delete
{
    internal class DeleteServerRentalCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteServerRentalCommand, Response>
    {
        public async Task<Response> Handle(DeleteServerRentalCommand request, CancellationToken cancellationToken)
        {
            var serverRentalRepo = unitOfWork.GetRepository<Domain.Entities.ServerRental>();
            var rental = await serverRentalRepo.GetByIdAsync(request.RentalId, cancellationToken);
            if (rental == null) return Response.Failure(Error.NotFound("Server rental not found"));

            await serverRentalRepo.DeleteAsync(request.RentalId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
    }
}