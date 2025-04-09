using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Create;

public class CreateConfigurationItemCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateConfigurationItemCommand, Response>
{
    public async Task<Response> Handle(CreateConfigurationItemCommand request, CancellationToken cancellationToken)
    {
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var deviceRepo = unitOfWork.GetRepository<Domain.Entities.Device>();

        // Kiểm tra Device tồn tại
        var device = await deviceRepo.GetByIdAsync(request.DeviceId, cancellationToken);
        if (device == null)
        {
            return Response.Failure(Error.NotFound("Device not found"));
        }

        // Kiểm tra trùng lặp cấu hình trong cùng một device
        var existingConfig = await configItemRepo.BuildQuery
            .Where(x => x.DeviceID == request.DeviceId && x.ConfigType == request.ConfigType)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingConfig != null)
        {
            return Error.Conflict($"Configuration item with type '{request.ConfigType}' already exists for this device");
        }

        // Mapping và tạo mới
        var newConfigItem = request.Adapt<Domain.Entities.ConfigurationItem>();
        newConfigItem.CreatedOn = DateTime.UtcNow;
        newConfigItem.CreatedDate = DateTime.UtcNow;

        await configItemRepo.CreateAsync(newConfigItem, cancellationToken);

        // Lưu changes
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}