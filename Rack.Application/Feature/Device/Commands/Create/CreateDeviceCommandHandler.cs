using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;
using Rack.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Rack.Contracts.Card.Response;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;
using Rack.Contracts.ConfigurationItem.Response;

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
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var cardRepo = unitOfWork.GetRepository<Domain.Entities.Card>();
        var portRepo = unitOfWork.GetRepository<Domain.Entities.Port>();
        var portConnectionRepo = unitOfWork.GetRepository<Domain.Entities.PortConnection>();

        // --- Validation ---
        var validationResult = await ValidateRequest(request, deviceRepository, deviceRackRepository, cancellationToken);
        if (!validationResult.IsSuccess)
            return Response<DeviceResponse>.Failure(validationResult.Error);

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // --- Tạo Device chính ---
            var newDevice = new Domain.Entities.Device
            {
                Name = request.Name,
                DeviceType = request.DeviceType,
                Status = request.Status,
                IpAddress = request.IpAddress ?? string.Empty,
                Manufacturer = request.Manufacturer,
                SerialNumber = request.SerialNumber,
                Model = request.Model,
                ParentDeviceID = request.ParentDeviceID,
                SlotInParent = request.SlotInParent,
                RackID = request.RackID,
                LinkIdPage = request.LinkIdPage,
                UPosition = request.UPosition,
                Size = request.Size,
            };
            await deviceRepository.CreateAsync(newDevice, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // --- Tạo các ConfigurationItem cho Device chính ---
            if (request.ConfigurationItems?.Any() == true)
            {
                var configItems = request.ConfigurationItems
                    .Select(ci => new Domain.Entities.ConfigurationItem
                    {
                        DeviceID = newDevice.Id,
                        ConfigType = ci.ConfigType,
                        ConfigValue = ci.ConfigValue,
                        Count = ci.Count
                    });
                await configItemRepo.InsertRangeAsync(configItems, cancellationToken);
            }

            // --- Tạo các Card, Port, PortConnection cho Device chính ---
            if (request.Cards?.Any() == true)
            {
                var clientPortIdToDbPortId = await CreateCardsAndPortsAsync(request.Cards, newDevice.Id, cardRepo, portRepo, cancellationToken);
                foreach (var card in request.Cards)
                {
                    if (card.Ports?.Any() == true)
                        await CreatePortConnectionsAsync(card.Ports, clientPortIdToDbPortId, portConnectionRepo, cancellationToken);
                }
            }

            // --- Tạo Ports và PortConnections trực tiếp cho Device (không qua Card) ---
            var devicePortIdMap = await CreateDevicePortsAsync(request.Ports, newDevice.Id, portRepo, cancellationToken);
            await CreatePortConnectionsAsync(request.Ports, devicePortIdMap, portConnectionRepo, cancellationToken);

            // Lưu toàn bộ thay đổi cho thiết bị chính và các phần liên quan
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // --- Tạo Child Devices nếu có ---
            if (request.ChildDevices?.Any() == true)
            {
                foreach (var child in request.ChildDevices)
                {
                    var childDevice = new Domain.Entities.Device
                    {
                        Name = child.Name,
                        DeviceType = child.DeviceType,
                        Status = child.Status,
                        IpAddress = child.IpAddress ?? string.Empty,
                        Manufacturer = child.Manufacturer,
                        SerialNumber = child.SerialNumber,
                        Model = child.Model,
                        ParentDeviceID = newDevice.Id,
                        SlotInParent = child.SlotInParent,
                        Size = 1,
                    };

                    await deviceRepository.CreateAsync(childDevice, cancellationToken);
                    await unitOfWork.SaveChangesAsync(cancellationToken);

                    // Các collection của Child Device
                    var needsSave = false;

                    // Tạo ConfigurationItems cho Child Device
                    if (child.ConfigurationItems?.Any() == true)
                    {
                        var childConfigItems = child.ConfigurationItems
                            .Select(ci => new Domain.Entities.ConfigurationItem
                            {
                                DeviceID = childDevice.Id,
                                ConfigType = ci.ConfigType,
                                ConfigValue = ci.ConfigValue,
                                Count = ci.Count
                            });
                        await configItemRepo.InsertRangeAsync(childConfigItems, cancellationToken);
                        needsSave = true;
                    }

                    // Tạo Cards cho Child Device
                    if (child.Cards?.Any() == true)
                    {
                        var clientPortIdToDbPortId = await CreateCardsAndPortsAsync(child.Cards, childDevice.Id, cardRepo, portRepo, cancellationToken);
                        foreach (var card in child.Cards)
                        {
                            if (card.Ports?.Any() == true)
                                await CreatePortConnectionsAsync(card.Ports, clientPortIdToDbPortId, portConnectionRepo, cancellationToken);
                        }
                        needsSave = true;
                    }

                    // Tạo Ports cho Child Device (không nằm trong card)
                    if (child.Ports?.Any() == true)
                    {
                        var childPorts = child.Ports
                            .Select(port => new Domain.Entities.Port
                            {
                                DeviceID = childDevice.Id,
                                CardID = port.CardID,
                                PortName = port.PortName,
                                PortType = port.PortType
                            });
                        await portRepo.InsertRangeAsync(childPorts, cancellationToken);
                        needsSave = true;
                    }

                    // Tạo PortConnections cho Child Device (không nằm trong card)
                    if (child.PortConnections?.Any() == true)
                    {
                        var childPortConnections = child.PortConnections
                            .Select(pc => new Domain.Entities.PortConnection
                            {
                                SourcePortID = devicePortIdMap[pc.SourcePortID],
                                DestinationPortID = pc.DestinationPortID,
                                CableType = pc.CableType,
                                Comment = pc.Comment
                            });
                        await portConnectionRepo.InsertRangeAsync(childPortConnections, cancellationToken);
                        needsSave = true;
                    }

                    // Chỉ lưu thay đổi nếu cần
                    if (needsSave)
                    {
                        await unitOfWork.SaveChangesAsync(cancellationToken);
                    }
                }
            }

            await unitOfWork.CommitAsync(cancellationToken);

            // Truy vấn lại device với các thông tin liên quan
            var device = await deviceRepository.BuildQuery
                .Include(d => d.ConfigurationItems)
                .Include(d => d.Cards).ThenInclude(c => c.Ports)
                .Include(d => d.Ports)
                .Include(d => d.ChildDevices)
                .FirstOrDefaultAsync(d => d.Id == newDevice.Id, cancellationToken);

            // Lấy tất cả PortConnections liên quan đến device
            var portIds = device.Ports.Select(p => p.Id).ToList();
            var cardPortIds = device.Cards.SelectMany(c => c.Ports).Select(p => p.Id).ToList();
            var allPortIds = portIds.Union(cardPortIds).ToList();
            var portConnections = await portConnectionRepo.BuildQuery
                .Where(pc => allPortIds.Contains(pc.SourcePortID) || allPortIds.Contains(pc.DestinationPortID))
                .ToListAsync(cancellationToken);

            // Map sang response giống GetById
            var deviceResponse = device.Adapt<DeviceResponse>();
            if (device.DeviceType.ToLower() == "server")
            {
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
                                        .Where(pc => pc.SourcePortID == p.Id || pc.DestinationPortID == p.Id)
                                        .Select(pc => pc.Adapt<PortConnectionResponse>())
                                        .ToList()
                                };
                                return portResponse;
                            }).ToList()
                        };
                        return cardResponse;
                    }).ToList(),
                    Ports = null,
                    PortConnections = null,
                    ChildDevices = device.ChildDevices?.Select(cd =>
                    {
                        var childResponse = cd.Adapt<DeviceResponse>();
                        childResponse = childResponse with
                        {
                            ConfigurationItems = cd.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                            Cards = cd.Cards?.Select(c => c.Adapt<CardResponse>()).ToList(),
                            Ports = cd.Ports?.Select(p => p.Adapt<PortResponse>()).ToList(),
                            PortConnections = null,
                            ChildDevices = null
                        };
                        return childResponse;
                    }).ToList()
                };
            }
            else
            {
                deviceResponse = deviceResponse with
                {
                    ConfigurationItems = device.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                    Cards = null,
                    Ports = device.Ports?.Select(p =>
                    {
                        var portResponse = p.Adapt<PortResponse>();
                        portResponse = portResponse with
                        {
                            PortConnections = portConnections
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
                            Cards = null,
                            Ports = cd.Ports?.Select(p => p.Adapt<PortResponse>()).ToList(),
                            PortConnections = null,
                            ChildDevices = null
                        };
                        return childResponse;
                    }).ToList()
                };
            }
            return Response<DeviceResponse>.Success(deviceResponse);
        }
        catch (Exception ex)
        {
            await unitOfWork.RollBackAsync(cancellationToken);
            return Response<DeviceResponse>.Failure(
                Error.Failure(message: $"An error occurred while creating the device: {ex.Message}"),
                HttpStatusCodeEnum.InternalServerError);
        }
    }

    private async Task<Response> ValidateRequest(
        CreateDeviceCommand request,
        IGenericRepository<Domain.Entities.Device> deviceRepository,
        IGenericRepository<Domain.Entities.DeviceRack> deviceRackRepository,
        CancellationToken cancellationToken)
    {
        // Validate Parent Device
        if (request.ParentDeviceID.HasValue)
        {
            var parentDevice = await deviceRepository.GetByIdAsync(request.ParentDeviceID.Value, cancellationToken);
            if (parentDevice == null)
                return Response.Failure(Error.NotFound(message: "Không tìm thấy thiết bị chứa"));

            if (request.RackID.HasValue || request.UPosition.HasValue)
                return Response.Failure(Error.Validation(message: "A device with a Parent cannot be directly assigned to a Rack/UPosition."));
        }

        // Validate Rack
        if (request.RackID.HasValue)
        {
            var rackExists = await deviceRackRepository.BuildQuery
                .Where(r => r.Id == request.RackID.Value && !r.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
            if (rackExists == null)
                return Response.Failure(Error.Validation(message: "Specified Rack does not exist."));

            // Validate UPosition
            if (request.UPosition.HasValue)
            {
                if (request.UPosition.Value < 1 || request.UPosition.Value > rackExists.Size)
                    return Response.Failure(Error.Validation(message: "UPosition is out of range for the specified rack."));
            }
        }

        // Validate Size
        if (request.Size <= 0)
            return Response.Failure(Error.Validation(message: "Device size must be greater than 0."));

        // Validate DeviceType
        if (string.IsNullOrWhiteSpace(request.DeviceType))
            return Response.Failure(Error.Validation(message: "Device type is required."));

        // Validate Cards and Ports
        if (request.Cards?.Any() == true && request.Ports?.Any() == true)
        {
            // Không thể xác định được Card ID một cách chính xác
            // vì CreateCardRequest không có trường để xác định ID duy nhất
            // Bỏ qua kiểm tra này
        }

        return Response.Success();
    }

    // --- Helper: Tạo Card và Port, mapping clientId -> dbId ---
    private async Task<Dictionary<Guid, Guid>> CreateCardsAndPortsAsync(
        List<Rack.Contracts.Card.Request.CreateCardRequest> cardRequests,
        Guid deviceId,
        IGenericRepository<Domain.Entities.Card> cardRepo,
        IGenericRepository<Domain.Entities.Port> portRepo,
        CancellationToken cancellationToken)
    {
        var clientPortIdToDbPortId = new Dictionary<Guid, Guid>();
        foreach (var cardRequest in cardRequests)
        {
            var newCard = new Domain.Entities.Card
            {
                DeviceID = deviceId,
                CardType = cardRequest.CardType,
                CardName = cardRequest.CardName,
                SerialNumber = cardRequest.SerialNumber
            };
            await cardRepo.CreateAsync(newCard, cancellationToken);
            // Tạo ports cho card nếu có
            if (cardRequest.Ports?.Any() == true)
            {
                var newPorts = new List<Domain.Entities.Port>();
                var portsWithRequestId = new Dictionary<Guid, Domain.Entities.Port>();
                foreach (var portRequest in cardRequest.Ports)
                {
                    var clientId = portRequest.DeviceID; // Không có Id, dùng DeviceID tạm nếu cần
                    var newPort = new Domain.Entities.Port
                    {
                        DeviceID = deviceId,
                        CardID = newCard.Id,
                        PortName = portRequest.PortName,
                        PortType = portRequest.PortType
                    };
                    if (clientId != Guid.Empty)
                        portsWithRequestId[clientId] = newPort;
                    newPorts.Add(newPort);
                }
                await portRepo.InsertRangeAsync(newPorts, cancellationToken);
                foreach (var kvp in portsWithRequestId)
                    clientPortIdToDbPortId[kvp.Key] = kvp.Value.Id;
            }
        }
        return clientPortIdToDbPortId;
    }

    // --- Helper: Tạo Port cho device-level ---
    private async Task<Dictionary<Guid, Guid>> CreateDevicePortsAsync(
        List<Rack.Contracts.Port.Request.CreatePortRequest> portRequests,
        Guid deviceId,
        IGenericRepository<Domain.Entities.Port> portRepo,
        CancellationToken cancellationToken)
    {
        var clientPortIdToDbPortId = new Dictionary<Guid, Guid>();
        var newPorts = new List<Domain.Entities.Port>();
        var portsWithRequestId = new Dictionary<Guid, Domain.Entities.Port>();
        foreach (var portRequest in portRequests)
        {
            var clientId = portRequest.DeviceID; // Không có Id, dùng DeviceID tạm nếu cần
            var newPort = new Domain.Entities.Port
            {
                DeviceID = deviceId,
                CardID = null,
                PortName = portRequest.PortName,
                PortType = portRequest.PortType
            };
            if (clientId != Guid.Empty)
                portsWithRequestId[clientId] = newPort;
            newPorts.Add(newPort);
        }
        await portRepo.InsertRangeAsync(newPorts, cancellationToken);
        foreach (var kvp in portsWithRequestId)
            clientPortIdToDbPortId[kvp.Key] = kvp.Value.Id;
        return clientPortIdToDbPortId;
    }

    // --- Helper: Tạo PortConnection cho từng port ---
    private async Task CreatePortConnectionsAsync(
        IEnumerable<Rack.Contracts.Port.Request.CreatePortRequest> portRequests,
        Dictionary<Guid, Guid> clientPortIdToDbPortId,
        IGenericRepository<Domain.Entities.PortConnection> portConnectionRepo,
        CancellationToken cancellationToken)
    {
        foreach (var portRequest in portRequests)
        {
            if (portRequest.PortConnections?.Any() == true && clientPortIdToDbPortId.TryGetValue(portRequest.DeviceID, out var dbPortId))
            {
                var newPortConnections = portRequest.PortConnections.Select(pc => new Domain.Entities.PortConnection
                {
                    SourcePortID = dbPortId,
                    DestinationPortID = pc.DestinationPortID,
                    CableType = pc.CableType,
                    Comment = pc.Comment
                }).ToList();
                await portConnectionRepo.InsertRangeAsync(newPortConnections, cancellationToken);
            }
        }
    }
}