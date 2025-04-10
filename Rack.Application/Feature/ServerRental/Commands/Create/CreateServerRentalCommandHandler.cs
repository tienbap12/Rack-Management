using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ServerRental.Commands.Create
{
    internal class CreateServerRentalCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateServerRentalCommand, Response>
    {
        public async Task<Response> Handle(CreateServerRentalCommand request, CancellationToken cancellationToken)
        {
            var serverRentalRepo = unitOfWork.GetRepository<Domain.Entities.ServerRental>();

            // Ki?m tra trùng l?p
            var existingRental = await serverRentalRepo.BuildQuery
                .Where(x => x.CustomerID == request.CustomerID && x.DeviceID == request.DeviceID)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingRental != null)
            {
                return Error.Conflict($"Server rental already exists for this customer and device.");
            }

            // Mapping và t?o m?i
            var newRental = request.Adapt<Domain.Entities.ServerRental>();
            await serverRentalRepo.CreateAsync(newRental, cancellationToken);

            // L?u changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
    }
}