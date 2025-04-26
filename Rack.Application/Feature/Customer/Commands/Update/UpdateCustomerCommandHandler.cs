using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Commands.Update
{
    internal class UpdateCustomerCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateCustomerCommand, Response>
    {
        public async Task<Response> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();
            var existingCustomer = await customerRepo.GetByIdAsync(request.CustomerId, cancellationToken);
            if (existingCustomer == null)
            {
                return Response.Failure(Error.NotFound());
            }

            existingCustomer.Name = request.Name;
            existingCustomer.ContactInfo = request.ContactInfo;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Response.Success();
        }
    }
}