using Mapster;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.Device.Commands.Update;

internal class UpdateDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDeviceCommand, Response<DeviceResponse>>
{
    public async Task<Response<DeviceResponse>> Handle(
        UpdateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();

            // Kiểm tra Device có tồn tại không
            var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);
            if (device == null)
            {
                return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy tủ rack của thiết bị này"));
            }

            // Kiểm tra ParentDevice nếu có
            if (request.ParentDeviceID.HasValue)
            {
                var parentDevice = await deviceRepository.GetByIdAsync(request.ParentDeviceID.Value, cancellationToken);
                if (parentDevice == null)
                {
                    return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy thiết bị chứa"));
                }
            }

            // Kiểm tra Rack nếu có
            if (request.RackID.HasValue)
            {
                var rack = await deviceRackRepository.GetByIdAsync(request.RackID.Value, cancellationToken);
                if (rack == null)
                {
                    return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy tủ rack của thiết bị này"));
                }
            }

            device.Manufacturer = request.Manufacturer ?? device.Manufacturer;
            device.Status = request.Status ?? device.Status;
            device.UPosition = request.uPosition;
            device.SlotInParent = request.SlotInParent ?? device.SlotInParent;
            device.DeviceType = request.DeviceType ?? device.DeviceType;
            device.IpAddress = request.IpAddress ?? device.IpAddress;
            device.Name = request.Name ?? device.Name;
            device.Model = request.Model ?? device.Model;
            device.SerialNumber = request.SerialNumber ?? device.SerialNumber;
            device.Size = request.Size ?? device.Size;
            device.RackID = request.RackID ?? device.RackID;
            device.ParentDeviceID = request.ParentDeviceID ?? device.ParentDeviceID;

            await unitOfWork.SaveChangesAsync(cancellationToken);
            var result = device.Adapt<DeviceResponse>();
            return Response<DeviceResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return Response<DeviceResponse>.Failure(Error.InternalServerError());
        }
    }
}