using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Delete;

public class DeleteConfigurationItemCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteConfigurationItemCommand, Response>
{
    public async Task<Response> Handle(DeleteConfigurationItemCommand request, CancellationToken cancellationToken)
    {
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var existConfig = await configItemRepo.GetByIdAsync(request.Id, cancellationToken);

        if (existConfig == null || existConfig.IsDeleted)
        {
            return Response.Failure(Error.NotFound());
        }

        existConfig.IsDeleted = true;
        existConfig.DeletedOn = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}