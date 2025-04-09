using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.DeviceRack.Commands.Delete;

internal class DeleteDeviceRackCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteDeviceRackCommand, Response>
{
    public async Task<Response> Handle(
        DeleteDeviceRackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();

            // Kiểm tra DeviceRack có tồn tại không
            var deviceRack = await deviceRackRepository.GetByIdAsync(request.DeviceRackId, cancellationToken);
            if (deviceRack == null)
            {
                return Response.Failure("DeviceRack not found");
            }

            // Kiểm tra xem có thiết bị nào đang sử dụng rack này không
            if (deviceRack.Devices.Any())
            {
                return Response.Failure("Cannot delete rack that has devices");
            }

            deviceRack.IsDeleted = true;
            deviceRack.DeletedOn = DateTime.UtcNow;
            deviceRack.DeletedBy = "System"; // TODO: Thay bằng user thực tế

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success("Xóa thiết bị thành công!");
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message);
        }
    }
}