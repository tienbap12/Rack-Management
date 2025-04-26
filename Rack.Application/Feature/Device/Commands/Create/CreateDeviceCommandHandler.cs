using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Commands.Create;

internal class CreateDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDeviceCommand, Response<DeviceResponse>>
{
    public async Task<Response<DeviceResponse>> Handle(
        CreateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
        var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();

        // Kiểm tra ParentDevice nếu có
        if (request.ParentDeviceID.HasValue)
        {
            var parentDevice = await deviceRepository.GetByIdAsync(request.ParentDeviceID.Value, cancellationToken);
            if (parentDevice == null)
            {
                return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy thiết bị chứa"));
            }
            if (request.RackID.HasValue || request.UPosition.HasValue) // Blade không thể có vị trí Rack
            {
                return Response<DeviceResponse>.Failure(Error.Validation(message: "A device with a Parent cannot be directly assigned to a Rack/UPosition."), HttpStatusCodeEnum.BadRequest);
            }
        }

        // Kiểm tra Rack nếu có
        if (request.RackID.HasValue)
        {
            var rackExists = await deviceRackRepository.BuildQuery.Where(r => r.Id == request.RackID.Value && !r.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            if (rackExists == null)
            {
                return Response<DeviceResponse>.Failure(Error.Validation(message: "Specified Rack does not exist."), HttpStatusCodeEnum.BadRequest);
            }
            if (!request.UPosition.HasValue || request.UPosition.Value < 1) // Nếu gắn rack thì UPosition là bắt buộc và > 0
            {
                return Response<DeviceResponse>.Failure(Error.Validation(message: "A valid UPosition is required when assigning to a Rack."), HttpStatusCodeEnum.BadRequest);
            }
        }
        else if (request.UPosition.HasValue) // Có UPosition nhưng không có RackID
        {
            return Response<DeviceResponse>.Failure(Error.Validation(message: "Cannot assign UPosition without a RackID."), HttpStatusCodeEnum.BadRequest);
        }
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var newDevice = request.Adapt<Domain.Entities.Device>();

            await deviceRepository.CreateAsync(newDevice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            var result = newDevice.Adapt<DeviceResponse>();
            return Response<DeviceResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return Response<DeviceResponse>.Failure(Error.Failure(message: $"An error occurred while creating the device: {ex.Message}"), HttpStatusCodeEnum.InternalServerError);
        }
    }
}