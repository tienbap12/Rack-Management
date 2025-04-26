using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Update;

public class UpdateConfigurationItemCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateConfigurationItemCommand, Response>
{
    public async Task<Response> Handle(UpdateConfigurationItemCommand request, CancellationToken cancellationToken)
    {
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();

        var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device == null || device.IsDeleted)
        {
            return Response.Failure(Error.NotFound());
        }

        var existConfig = await configItemRepo.GetByIdAsync(request.Id, cancellationToken);

        if (existConfig == null || existConfig.IsDeleted)
        {
            return Response.Failure(Error.NotFound());
        }

        // Kiểm tra trùng lặp nếu thay đổi ConfigType
        if (existConfig.ConfigType != request.ConfigType)
        {
            var existingConfig = await configItemRepo.BuildQuery
                .Where(x => x.DeviceID == existConfig.DeviceID && x.ConfigType == request.ConfigType)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingConfig != null)
            {
                return Response.Failure(Error.Conflict());
            }
        }

        existConfig.ConfigType = request.ConfigType;
        existConfig.ConfigValue = request.ConfigValue;

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}