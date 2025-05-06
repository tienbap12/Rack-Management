using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Card.Response;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Device.Queries.GetById;

internal class GetDeviceByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetDeviceByIdQuery, DeviceResponse>
{
    public async Task<Response<DeviceResponse>> Handle(GetDeviceByIdQuery request, CancellationToken cancellationToken)
    {
        var deviceRepository = unitOfWork.GetRepository<Rack.Domain.Entities.Device>();
        var portConnectionRepository = unitOfWork.GetRepository<Rack.Domain.Entities.PortConnection>();

        // Lấy device với các thông tin liên quan
        var device = await deviceRepository.BuildQuery
            .Include(d => d.ConfigurationItems)
            .Include(d => d.Cards)
                .ThenInclude(c => c.Ports)
            .Include(d => d.Ports)
            .Include(d => d.ChildDevices)
            .FirstOrDefaultAsync(d => d.Id == request.Id && !d.IsDeleted, cancellationToken);

        if (device is null)
        {
            return Response<DeviceResponse>.Failure(Error.NotFound());
        }

        // Lấy tất cả PortConnections liên quan đến device
        var portIds = device.Ports.Select(p => p.Id).ToList();
        var cardPortIds = device.Cards.SelectMany(c => c.Ports).Select(p => p.Id).ToList();
        var allPortIds = portIds.Union(cardPortIds).ToList();

        var portConnections = await portConnectionRepository.BuildQuery
            .Where(pc => allPortIds.Contains(pc.SourcePortID) || allPortIds.Contains(pc.DestinationPortID))
            .ToListAsync(cancellationToken);

        // Map sang response
        var deviceResponse = device.Adapt<DeviceResponse>();

        // Sử dụng with để gán các property init-only
        deviceResponse = deviceResponse with
        {
            ConfigurationItems = device.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
            Cards = device.Cards?.Select(c =>
            {
                var cardResponse = c.Adapt<CardResponse>();
                cardResponse = cardResponse with
                {
                    Ports = c.Ports?.Select(p =>
                    {
                        var portResponse = p.Adapt<PortResponse>();
                        portResponse = portResponse with
                        {
                            Connections = portConnections
                                .Where(pc => pc.SourcePortID == p.Id || pc.DestinationPortID == p.Id)
                                .Select(pc => pc.Adapt<PortConnectionResponse>())
                                .ToList()
                        };
                        return portResponse;
                    }).ToList()
                };
                return cardResponse;
            }).ToList(),
            Ports = device.Ports?.Select(p =>
            {
                var portResponse = p.Adapt<PortResponse>();
                portResponse = portResponse with
                {
                    Connections = portConnections
                        .Where(pc => pc.SourcePortID == p.Id || pc.DestinationPortID == p.Id)
                        .Select(pc => pc.Adapt<PortConnectionResponse>())
                        .ToList()
                };
                return portResponse;
            }).ToList(),
            PortConnections = portConnections.Select(pc => pc.Adapt<PortConnectionResponse>()).ToList(),
            ChildDevices = device.ChildDevices?.Select(cd =>
            {
                var childResponse = cd.Adapt<DeviceResponse>();
                childResponse = childResponse with
                {
                    ConfigurationItems = cd.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                    Cards = cd.Cards?.Select(c => c.Adapt<CardResponse>()).ToList(),
                    Ports = cd.Ports?.Select(p => p.Adapt<PortResponse>()).ToList(),
                    PortConnections = portConnections.Select(pc => pc.Adapt<PortConnectionResponse>()).ToList(),
                    ChildDevices = null // Không lấy nested child devices
                };
                return childResponse;
            }).ToList()
        };

        return Response<DeviceResponse>.Success(deviceResponse);
    }
}