using Mapster;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Customer.Commands.Create
{
    internal class CreateCustomerCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateCustomerCommand, Response>
    {
        public async Task<Response> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
        {
            var customerRepo = unitOfWork.GetRepository<Domain.Entities.Customer>();

            var newCustomer = request.Adapt<Domain.Entities.Customer>();
            await customerRepo.CreateAsync(newCustomer, cancellationToken);

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
    }
}