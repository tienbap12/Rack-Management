using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Commands.Update
{
    internal class UpdateCustomerCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateCustomerCommand, Response>
    {
        public async Task<Response> Handle(UpdateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();
            
            // Sử dụng GetByIdWithTrackingAsync vì cần tracking để cập nhật
            var existingCustomer = await customerRepo.GetByIdWithTrackingAsync(request.CustomerId, cancellationToken);
            if (existingCustomer == null)
            {
                return Response.Failure(Error.NotFound());
            }

            existingCustomer.Name = request.Name;
            existingCustomer.ContactInfo = request.ContactInfo;

            // Sử dụng UpdateAsync thay vì SaveChangesAsync
            await customerRepo.UpdateAsync(existingCustomer, cancellationToken);
            return Response.Success();
        }
    }
}