using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Commands.Create
{
    internal class CreateCustomerCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateCustomerCommand, Response>
    {
        public async Task<Response> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();

            // Ki?m tra trùng l?p
            var existingCustomer = await customerRepo.BuildQuery
                .Where(x => x.Name == request.Name)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingCustomer != null)
            {
                return Error.Conflict($"Customer with name '{request.Name}' already exists");
            }

            // Mapping và t?o m?i
            var newCustomer = request.Adapt<Domain.Entities.Customer>();
            await customerRepo.CreateAsync(newCustomer, cancellationToken);

            // L?u changes
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
    }
}