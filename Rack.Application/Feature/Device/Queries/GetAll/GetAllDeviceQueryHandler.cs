using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Card.Response;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Queries.GetAll;

internal class GetAllDeviceQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllDeviceQuery, List<DeviceResponse>>
{
    public async Task<Response<List<DeviceResponse>>> Handle(
        GetAllDeviceQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
            var portConnectionRepository = unitOfWork.GetRepository<Domain.Entities.PortConnection>();

            // Lấy danh sách devices với các thông tin liên quan
            var devices = await deviceRepository.BuildQuery
                .Include(d => d.ConfigurationItems)
                .Include(d => d.Cards)
                    .ThenInclude(c => c.Ports)
                .Include(d => d.Ports)
                .Include(d => d.ChildDevices)
                .Where(d => !d.IsDeleted)
                .Where(d => request.Id == null || (request.Id == Guid.Empty ? d.RackID == null : d.RackID == request.Id))
                .Where(d => request.Status == null || d.Status == request.Status)
                .ToListAsync(cancellationToken);

            // Lấy tất cả PortIds từ tất cả devices
            var allPortIds = devices
                .SelectMany(d => d.Ports.Select(p => p.Id))
                .Union(devices.SelectMany(d => d.Cards.SelectMany(c => c.Ports.Select(p => p.Id))))
                .ToList();

            // Lấy tất cả PortConnections liên quan
            var portConnections = await portConnectionRepository.BuildQuery
                .Where(pc => allPortIds.Contains(pc.SourcePortID) || allPortIds.Contains(pc.DestinationPortID))
                .ToListAsync(cancellationToken);

            // Map sang response
            var deviceResponses = devices.Select(device =>
            {
                var deviceResponse = device.Adapt<DeviceResponse>();
                return deviceResponse with
                {
                    ConfigurationItems = device.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                    Cards = device.Cards?.Select(c =>
                    {
                        var cardResponse = c.Adapt<CardResponse>();
                        return cardResponse with
                        {
                            Ports = c.Ports?.Select(p =>
                            {
                                var portResponse = p.Adapt<PortResponse>();
                                return portResponse with
                                {
                                    Connections = portConnections
                                        .Where(pc => pc.SourcePortID == p.Id || pc.DestinationPortID == p.Id)
                                        .Select(pc => pc.Adapt<PortConnectionResponse>())
                                        .ToList()
                                };
                            }).ToList()
                        };
                    }).ToList(),
                    Ports = device.Ports?.Select(p =>
                    {
                        var portResponse = p.Adapt<PortResponse>();
                        return portResponse with
                        {
                            Connections = portConnections
                                .Where(pc => pc.SourcePortID == p.Id || pc.DestinationPortID == p.Id)
                                .Select(pc => pc.Adapt<PortConnectionResponse>())
                                .ToList()
                        };
                    }).ToList(),
                    PortConnections = portConnections.Select(pc => pc.Adapt<PortConnectionResponse>()).ToList(),
                    ChildDevices = device.ChildDevices?.Select(cd =>
                    {
                        var childResponse = cd.Adapt<DeviceResponse>();
                        return childResponse with
                        {
                            ConfigurationItems = cd.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                            Cards = cd.Cards?.Select(c => c.Adapt<CardResponse>()).ToList(),
                            Ports = cd.Ports?.Select(p => p.Adapt<PortResponse>()).ToList(),
                            PortConnections = portConnections.Select(pc => pc.Adapt<PortConnectionResponse>()).ToList(),
                            ChildDevices = null // Không lấy nested child devices
                        };
                    }).ToList()
                };
            }).ToList();

            return Response<List<DeviceResponse>>.Success(deviceResponses);
        }
        catch (Exception ex)
        {
            return Response<List<DeviceResponse>>.Failure(Error.Failure());
        }
    }
}