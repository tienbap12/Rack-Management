using Microsoft.EntityFrameworkCore;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Feature.DeviceRack.Queries.GetDeviceRackById;

internal class GetDeviceRackByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDeviceRackByIdQuery, DeviceRackResponse>
{
    public async Task<Response<DeviceRackResponse>> Handle(GetDeviceRackByIdQuery request, CancellationToken cancellationToken)
    {
        var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
        var rackDto = await deviceRackRepository.BuildQuery
            .Include(r => r.DataCenter)
            .Include(r => r.Devices)
            .Where(r => r.Id == request.RackId)
            .Select(r => new DeviceRackResponse
            {
                Id = r.Id,
                DataCenterID = r.DataCenterID,
                RackNumber = r.RackNumber,
                Size = r.Size,
                DataCenterName = r.DataCenter.Name,
                DataCenterLocation = r.DataCenter.Location,
                Devices = r.Devices
                    .Where(d => !d.IsDeleted && d.Status == DeviceStatus.Active)
                    .Select(d => new Rack.Contracts.Device.Responses.DeviceResponse
                    {
                        Id = d.Id,
                        ParentDeviceID = d.ParentDeviceID,
                        RackID = d.RackID,
                        RackName = r.RackNumber,
                        Size = d.Size,
                        Name = d.Name,
                        IpAddress = d.IpAddress,
                        UPosition = d.UPosition,
                        SlotInParent = d.SlotInParent,
                        DeviceType = d.DeviceType.ToString(),
                        Manufacturer = d.Manufacturer,
                        SerialNumber = d.SerialNumber,
                        Model = d.Model,
                        Status = d.Status,
                        // Các trường collection (Cards, Ports, ConfigurationItems, ChildDevices, PortConnections) nếu cần, có thể select tiếp ở đây
                    }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (rackDto == null)
            return Response<DeviceRackResponse>.Failure(Error.NotFound(message: "Không tìm thấy tủ rack này"), HttpStatusCodeEnum.NotFound);

        return Response<DeviceRackResponse>.Success(rackDto);
    }
}