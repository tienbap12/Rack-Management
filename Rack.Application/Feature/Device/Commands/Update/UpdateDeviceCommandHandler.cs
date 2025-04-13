using Mapster;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Commands.Update;

internal class UpdateDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDeviceCommand, Response>
{
    public async Task<Response> Handle(
        UpdateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();

            // Kiểm tra Device có tồn tại không
            var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);
            if (device == null)
            {
                return Response<DeviceResponse>.Failure("Device not found");
            }

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
                    return Response<DeviceResponse>.Failure("Không tìm thấy tủ rack của thiết bị này");
                }
            }

            request.Adapt(device);

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