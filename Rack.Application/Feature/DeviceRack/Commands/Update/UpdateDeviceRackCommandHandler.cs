using Mapster;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DeviceRack.Commands.Update;

internal class UpdateDeviceRackCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDeviceRackCommand, Response>
{
    public async Task<Response> Handle(
        UpdateDeviceRackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
            var dataCenterRepository = unitOfWork.GetRepository<Domain.Entities.DataCenter>();

            // Kiểm tra DeviceRack có tồn tại không
            var deviceRack = await deviceRackRepository.GetByIdAsync(request.RackId, cancellationToken);
            if (deviceRack == null)
            {
                return Response.Failure("DeviceRack not found");
            }

            // Kiểm tra DataCenter có tồn tại không
            var dataCenter = await dataCenterRepository.GetByIdAsync(request.DataCenterID, cancellationToken);
            if (dataCenter == null)
            {
                return Response.Failure("DataCenter not found");
            }

            request.Adapt(deviceRack);
            deviceRack.LastModifiedOn = DateTime.UtcNow;
            deviceRack.LastModifiedBy = "System"; // TODO: Thay bằng user thực tế

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message);
        }
    }
}