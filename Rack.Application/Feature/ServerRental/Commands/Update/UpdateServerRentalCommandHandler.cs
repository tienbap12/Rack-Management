using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ServerRental.Commands.Update
{
    internal class UpdateServerRentalCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateServerRentalCommand, Response>
    {
        public async Task<Response> Handle(UpdateServerRentalCommand request, CancellationToken cancellationToken)
        {
            var serverRentalRepo = unitOfWork.GetRepository<Domain.Entities.ServerRental>();
            var existingRental = await serverRentalRepo.GetByIdAsync(request.RentalId, cancellationToken);
            if (existingRental == null)
            {
                return Response.Failure(Error.NotFound(message: "Server rental not found"));
            }

            existingRental.StartDate = request.StartDate;
            existingRental.EndDate = request.EndDate;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Response.Success();
        }
    }
}