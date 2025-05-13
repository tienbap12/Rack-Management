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
            .Include(d => d.Rack)
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
            .Include(pc => pc.SourcePort)
                .ThenInclude(p => p.Device)
            .Include(pc => pc.DestinationPort)
                .ThenInclude(p => p.Device)
            .Include(pc => pc.SourcePort.Device.Rack)
            .Include(pc => pc.DestinationPort.Device.Rack)
            .Where(pc => allPortIds.Contains(pc.SourcePortID) || allPortIds.Contains(pc.DestinationPortID))
            .ToListAsync(cancellationToken);

        // Map sang response
        var deviceResponse = device.Adapt<DeviceResponse>();
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
                            PortConnections = portConnections
                                .Where(pc => pc.SourcePortID == p.Id)
                                .Select(pc => new PortConnectionResponse
                                {
                                    Id = pc.Id,
                                    SourcePortID = pc.SourcePortID,
                                    DestinationPortID = pc.DestinationPortID,
                                    CableType = pc.CableType,
                                    Comment = pc.Comment,
                                    SourceDevice = pc.SourcePort?.Device != null ? new SimpleDeviceDto {
                                        Id = pc.SourcePort.Device.Id,
                                        RackName = pc.SourcePort.Device.Rack?.RackNumber,
                                        Slot = pc.SourcePort.Device.UPosition?.ToString(),
                                        DeviceName = pc.SourcePort.Device.Name
                                    } : null,
                                    SourcePort = pc.SourcePort?.Adapt<PortResponse>(),
                                    DestinationDevice = pc.DestinationPort?.Device != null ? new SimpleDeviceDto {
                                        Id = pc.DestinationPort.Device.Id,
                                        RackName = pc.DestinationPort.Device.Rack?.RackNumber,
                                        Slot = pc.DestinationPort.Device.UPosition?.ToString(),
                                        DeviceName = pc.DestinationPort.Device.Name
                                    } : null,
                                    DestinationPort = pc.DestinationPort?.Adapt<PortResponse>(),
                                    RemoteDevice = pc.DestinationPort?.Device != null ? new SimpleDeviceDto {
                                        Id = pc.DestinationPort.Device.Id,
                                        RackName = pc.DestinationPort.Device.Rack?.RackNumber,
                                        Slot = pc.DestinationPort.Device.UPosition?.ToString(),
                                        DeviceName = pc.DestinationPort.Device.Name
                                    } : null,
                                    RemotePort = pc.DestinationPort?.Adapt<PortResponse>(),
                                    CreatedBy = pc.CreatedBy,
                                    CreatedOn = pc.CreatedOn,
                                    LastModifiedBy = pc.LastModifiedBy,
                                    LastModifiedOn = pc.LastModifiedOn,
                                    IsDeleted = pc.IsDeleted,
                                    DeletedBy = pc.DeletedBy,
                                    DeletedOn = pc.DeletedOn
                                })
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
                    PortConnections = portConnections
                        .Where(pc => pc.SourcePortID == p.Id)
                        .Select(pc => new PortConnectionResponse
                        {
                            Id = pc.Id,
                            SourcePortID = pc.SourcePortID,
                            DestinationPortID = pc.DestinationPortID,
                            CableType = pc.CableType,
                            Comment = pc.Comment,
                            SourceDevice = pc.SourcePort?.Device != null ? new SimpleDeviceDto {
                                Id = pc.SourcePort.Device.Id,
                                RackName = pc.SourcePort.Device.Rack?.RackNumber,
                                Slot = pc.SourcePort.Device.UPosition?.ToString(),
                                DeviceName = pc.SourcePort.Device.Name
                            } : null,
                            SourcePort = pc.SourcePort?.Adapt<PortResponse>(),
                            DestinationDevice = pc.DestinationPort?.Device != null ? new SimpleDeviceDto {
                                Id = pc.DestinationPort.Device.Id,
                                RackName = pc.DestinationPort.Device.Rack?.RackNumber,
                                Slot = pc.DestinationPort.Device.UPosition?.ToString(),
                                DeviceName = pc.DestinationPort.Device.Name
                            } : null,
                            DestinationPort = pc.DestinationPort?.Adapt<PortResponse>(),
                            RemoteDevice = pc.DestinationPort?.Device != null ? new SimpleDeviceDto {
                                Id = pc.DestinationPort.Device.Id,
                                RackName = pc.DestinationPort.Device.Rack?.RackNumber,
                                Slot = pc.DestinationPort.Device.UPosition?.ToString(),
                                DeviceName = pc.DestinationPort.Device.Name
                            } : null,
                            RemotePort = pc.DestinationPort?.Adapt<PortResponse>(),
                            CreatedBy = pc.CreatedBy,
                            CreatedOn = pc.CreatedOn,
                            LastModifiedBy = pc.LastModifiedBy,
                            LastModifiedOn = pc.LastModifiedOn,
                            IsDeleted = pc.IsDeleted,
                            DeletedBy = pc.DeletedBy,
                            DeletedOn = pc.DeletedOn
                        })
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