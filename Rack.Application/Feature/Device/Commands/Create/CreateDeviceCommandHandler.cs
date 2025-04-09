using Mapster;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Commands.Create;

internal class CreateDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDeviceCommand, Response>
{
    public async Task<Response> Handle(
        CreateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();

            // Kiểm tra ParentDevice nếu có
            if (request.ParentDeviceID.HasValue)
            {
                var parentDevice = await deviceRepository.GetByIdAsync(request.ParentDeviceID.Value, cancellationToken);
                if (parentDevice == null)
                {
                    return Response<DeviceResponse>.Failure("Parent device not found");
                }
            }

            // Kiểm tra Rack nếu có
            if (request.RackID.HasValue)
            {
                var rack = await deviceRackRepository.GetByIdAsync(request.RackID.Value, cancellationToken);
                if (rack == null)
                {
                    return Response<DeviceResponse>.Failure("Rack not found");
                }
            }

            var device = request.Adapt<Domain.Entities.Device>();
            device.CreatedOn = DateTime.UtcNow;
            device.CreatedBy = "System";

            await deviceRepository.CreateAsync(device, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var result = device.Adapt<DeviceResponse>();
            return Response<DeviceResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return Response<DeviceResponse>.Failure(ex.Message);
        }
    }
}