using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Commands.Delete
{
    internal class DeleteCustomerCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteCustomerCommand, Response>
    {
        public async Task<Response> Handle(DeleteCustomerCommand request, CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();
            var customer = await customerRepo.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null) return Response.Failure(Error.NotFound("Customer not found"));

            await customerRepo.DeleteAsync(request.CustomerId, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
    }
}