using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Update;

public class UpdateConfigurationItemCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateConfigurationItemCommand, Response>
{
    public async Task<Response> Handle(UpdateConfigurationItemCommand request, CancellationToken cancellationToken)
    {
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var existConfig = await configItemRepo.GetByIdAsync(request.Id, cancellationToken);

        if (existConfig == null || existConfig.IsDeleted)
        {
            return Response.Failure(Error.NotFound("Configuration item not found"));
        }

        // Kiểm tra trùng lặp nếu thay đổi ConfigType
        if (existConfig.ConfigType != request.ConfigType)
        {
            var existingConfig = await configItemRepo.BuildQuery
                .Where(x => x.DeviceID == existConfig.DeviceID && x.ConfigType == request.ConfigType)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingConfig != null)
            {
                return Error.Conflict($"Configuration item with type '{request.ConfigType}' already exists for this device");
            }
        }

        existConfig.ConfigType = request.ConfigType;
        existConfig.ConfigValue = request.ConfigValue;
        existConfig.LastModifiedBy = request.LastModifiedBy;
        existConfig.LastModifiedOn = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}