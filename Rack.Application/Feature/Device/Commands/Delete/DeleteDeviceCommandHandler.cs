using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Commands.Delete;

internal class DeleteDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteDeviceCommand, Response>
{
    public async Task<Response> Handle(
        DeleteDeviceCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();

            // Kiểm tra Device có tồn tại không
            var device = await deviceRepository.GetByIdAsync(request.Id, cancellationToken);
            if (device == null)
            {
                return Response.Failure("Device not found");
            }

            // Kiểm tra xem có thiết bị con nào không
            if (device.ChildDevices.Any())
            {
                return Response.Failure("Cannot delete device that has child devices");
            }

            // Kiểm tra xem có configuration items nào không
            if (device.ConfigurationItems.Any())
            {
                return Response.Failure("Cannot delete device that has configuration items");
            }

            // Kiểm tra xem có server rentals nào không
            if (device.ServerRentals.Any())
            {
                return Response.Failure("Cannot delete device that has server rentals");
            }

            device.IsDeleted = true;
            device.DeletedOn = DateTime.UtcNow;
            device.DeletedBy = "System"; // TODO: Thay bằng user thực tế

            await deviceRepository.DeleteAsync(device.Id, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message);
        }
    }
}