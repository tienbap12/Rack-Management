using Mapster;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DeviceRack.Commands.Create;

internal class CreateDeviceRackCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<CreateDeviceRackCommand, Response>
{
    public async Task<Response> Handle(
        CreateDeviceRackCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
            var dataCenterRepository = unitOfWork.GetRepository<Domain.Entities.DataCenter>();

            // Kiểm tra DataCenter có tồn tại không
            var dataCenter = await dataCenterRepository.GetByIdAsync(request.Request.DataCenterID, cancellationToken);
            if (dataCenter == null)
            {
                return Response.Failure("DataCenter not found");
            }

            var deviceRack = request.Request.Adapt<Domain.Entities.DeviceRack>();
            deviceRack.CreatedOn = DateTime.UtcNow;
            deviceRack.CreatedBy = "System"; // TODO: Thay bằng user thực tế

            await deviceRackRepository.CreateAsync(deviceRack, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success();
        }
        catch (Exception ex)
        {
            return Response.Failure(ex.Message);
        }
    }
}