using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using System.Reflection;
using Rack.Contracts.Card.Response;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;
using Rack.Contracts.ConfigurationItem.Response;
using Rack.Contracts.Port.Request;
using Rack.Contracts.PortConnection.Request;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Commands.Update;

internal class UpdateDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDeviceCommand, Response<DeviceResponse>>
{
    public async Task<Response<DeviceResponse>> Handle(
        UpdateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(request.Request);
        var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
        var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var cardRepo = unitOfWork.GetRepository<Domain.Entities.Card>();
        var portRepo = unitOfWork.GetRepository<Domain.Entities.Port>();
        var portConnectionRepo = unitOfWork.GetRepository<Domain.Entities.PortConnection>();

        try
        {
            var device = await deviceRepository.GetByIdAsync(request.DeviceId, cancellationToken);

            if (device == null)
                return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy thiết bị cần cập nhật."));

            // --- Kiểm tra Parent Device và Rack ---
            if (request.Request.ParentDeviceID.HasValue)
            {
                var parentDevice = await deviceRepository.GetByIdAsync(request.Request.ParentDeviceID.Value, cancellationToken);
                if (parentDevice == null)
                    return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy thiết bị cha."));
            }

            if (request.Request.RackID.HasValue)
            {
                var rack = await deviceRackRepository.GetByIdAsync(request.Request.RackID.Value, cancellationToken);
                if (rack == null)
                    return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy tủ rack."));
            }

            // --- Update Properties ---
            device.Manufacturer = request.Request.Manufacturer ?? device.Manufacturer;
            device.Status = request.Request.Status ?? device.Status;
            if (request.Request.Status == DeviceStatus.Inventory)
            {
                device.UPosition = null;
            }
            else if (request.Request.UPosition.HasValue)
            {
                device.UPosition = request.Request.UPosition.Value;
            }
            device.SlotInParent = request.Request.SlotInParent ?? device.SlotInParent;
            device.DeviceType = request.Request.DeviceType ?? device.DeviceType;
            device.IpAddress = request.Request.IpAddress ?? device.IpAddress;
            device.LinkIdPage = request.Request.LinkIdPage ?? device.LinkIdPage;
            device.Name = request.Request.Name ?? device.Name;
            device.Model = request.Request.Model ?? device.Model;
            device.SerialNumber = request.Request.SerialNumber ?? device.SerialNumber;
            device.Size = request.Request.Size ?? device.Size;
            device.RackID = request.Request.RackID ?? device.RackID;
            device.ParentDeviceID = request.Request.ParentDeviceID ?? device.ParentDeviceID;
            // Log trạng thái entity trước khi lưu
            await deviceRepository.UpdateAsync(device, cancellationToken);
            Console.WriteLine($"[UpdateDevice] UPosition after set: {device.UPosition}");
            // --- Update ConfigurationItems ---
            if (request.Request.ConfigurationItems?.Any() == true)
            {
                var oldConfigs = configItemRepo.BuildQuery.Where(c => c.DeviceID == device.Id);
                await configItemRepo.DeleteRangeAsync(oldConfigs, cancellationToken);

                var newConfigs = request.Request.ConfigurationItems
                    .Select(ci => new Domain.Entities.ConfigurationItem
                    {
                        DeviceID = device.Id,
                        ConfigType = ci.ConfigType,
                        ConfigValue = ci.ConfigValue,
                        Count = ci.Count
                    });

                await configItemRepo.InsertRangeAsync(newConfigs, cancellationToken);
            }
            Console.WriteLine($"[UpdateDevice] UPosition after config: {device.UPosition}");
            // --- Update Cards ---
            if (request.Request.Cards != null)
            {
                // Xóa tất cả port connections, ports, cards nếu mảng rỗng hoặc có phần tử (tức là client muốn xóa hoặc cập nhật lại)
                var oldCardsIds = await cardRepo.BuildQuery
                    .Where(c => c.DeviceID == device.Id)
                    .Select(c => c.Id)
                    .ToListAsync(cancellationToken);
                var oldPortIds = await portRepo.BuildQuery
                    .Where(p => p.CardID.HasValue && oldCardsIds.Contains(p.CardID.Value))
                    .Select(p => p.Id)
                    .ToListAsync(cancellationToken);
                var oldPortConnections = portConnectionRepo.BuildQuery
                    .Where(pc => oldPortIds.Contains(pc.SourcePortID) || oldPortIds.Contains(pc.DestinationPortID));
                await portConnectionRepo.DeleteRangeAsync(oldPortConnections, cancellationToken);
                var oldPorts = portRepo.BuildQuery
                    .Where(p => p.CardID.HasValue && oldCardsIds.Contains(p.CardID.Value));
                await portRepo.DeleteRangeAsync(oldPorts, cancellationToken);
                var oldCards = cardRepo.BuildQuery.Where(c => c.DeviceID == device.Id);
                await cardRepo.DeleteRangeAsync(oldCards, cancellationToken);

                if (request.Request.Cards.Count > 0)
                {
                    var clientPortIdToDbPortId = await CreateCardsAndPortsAsync(request.Request.Cards, device.Id, cardRepo, portRepo, cancellationToken);
                    foreach (var card in request.Request.Cards)
                    {
                        if (card.Ports?.Any() == true)
                            await CreatePortConnectionsAsync(card.Ports, clientPortIdToDbPortId, portConnectionRepo, cancellationToken);
                    }
                }
            }
            Console.WriteLine($"[UpdateDevice] UPosition after cards: {device.UPosition}");
            // --- Update Ports directly attached to device (not to cards) ---
            if (request.Request.Ports != null)
            {
                var oldPorts = portRepo.BuildQuery.Where(p => p.DeviceID == device.Id && p.CardID == null);
                await portRepo.DeleteRangeAsync(oldPorts, cancellationToken);
                if (request.Request.Ports.Count > 0)
                {
                    var clientPortIdToDbPortId = await CreateDevicePortsAsync(request.Request.Ports, device.Id, portRepo, cancellationToken);
                    await CreatePortConnectionsAsync(request.Request.Ports, clientPortIdToDbPortId, portConnectionRepo, cancellationToken);
                }
            }
            Console.WriteLine($"[UpdateDevice] UPosition after ports: {device.UPosition}");
            // --- Update Child Devices ---
            if (request.Request.ChildDevices?.Any() == true)
            {
                var oldChildDevices = deviceRepository.BuildQuery
                    .Where(d => d.ParentDeviceID == device.Id);

                await deviceRepository.DeleteRangeAsync(oldChildDevices, cancellationToken);

                foreach (var child in request.Request.ChildDevices)
                {
                    var newChildDevice = new Domain.Entities.Device
                    {
                        Name = child.Name,
                        DeviceType = child.DeviceType,
                        Status = child.Status,
                        IpAddress = child.IpAddress ?? string.Empty,
                        Manufacturer = child.Manufacturer,
                        SerialNumber = child.SerialNumber,
                        Model = child.Model,
                        ParentDeviceID = device.Id,
                        SlotInParent = child.SlotInParent,
                        Size = 1
                    };

                    await deviceRepository.CreateAsync(newChildDevice, cancellationToken);
                }
            }
            Console.WriteLine($"[UpdateDevice] UPosition after child devices: {device.UPosition}");
            await unitOfWork.SaveChangesAsync();

            // Truy vấn lại device với các thông tin liên quan
            var updatedDevice = await deviceRepository.BuildQuery
                .Include(d => d.ConfigurationItems)
                .Include(d => d.Cards).ThenInclude(c => c.Ports)
                .Include(d => d.Ports)
                .Include(d => d.ChildDevices)
                .FirstOrDefaultAsync(d => d.Id == device.Id, cancellationToken);
            Console.WriteLine($"[UpdateDevice] UPosition in DB after save: {updatedDevice.UPosition}");
            // Lấy tất cả PortConnections liên quan đến device
            var portIds = updatedDevice.Ports.Select(p => p.Id).ToList();
            var cardPortIds = updatedDevice.Cards.SelectMany(c => c.Ports).Select(p => p.Id).ToList();
            var allPortIds = portIds.Union(cardPortIds).ToList();
            var portConnections = await portConnectionRepo.BuildQuery
                .Where(pc => allPortIds.Contains(pc.SourcePortID) || allPortIds.Contains(pc.DestinationPortID))
                .ToListAsync(cancellationToken);

            // Map sang response giống GetById/Create
            var deviceResponse = updatedDevice.Adapt<DeviceResponse>();
            if (updatedDevice.DeviceType.ToLower() == "server")
            {
                deviceResponse = deviceResponse with
                {
                    ConfigurationItems = updatedDevice.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                    Cards = updatedDevice.Cards?.Select(c =>
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
                    ChildDevices = updatedDevice.ChildDevices?.Select(cd =>
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
                    ConfigurationItems = updatedDevice.ConfigurationItems?.Select(ci => ci.Adapt<ConfigurationItemResponse>()).ToList(),
                    Cards = null,
                    Ports = updatedDevice.Ports?.Select(p =>
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
                    ChildDevices = updatedDevice.ChildDevices?.Select(cd =>
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
            // Log the error with details
            Console.WriteLine($"Error updating device: {ex.Message}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }

            return Response<DeviceResponse>.Failure(Error.Failure(message: $"Có lỗi xảy ra: {ex.Message}"));
        }
    }

    // --- Helper: Tạo Card và Port, mapping clientId -> dbId ---
    private async Task<Dictionary<Guid, Guid>> CreateCardsAndPortsAsync(
        List<Rack.Contracts.Card.Request.CreateCardRequest> cardRequests,
        Guid deviceId,
        Rack.Domain.Interfaces.IGenericRepository<Rack.Domain.Entities.Card> cardRepo,
        Rack.Domain.Interfaces.IGenericRepository<Rack.Domain.Entities.Port> portRepo,
        CancellationToken cancellationToken)
    {
        var clientPortIdToDbPortId = new Dictionary<Guid, Guid>();
        foreach (var cardRequest in cardRequests)
        {
            var newCard = new Card
            {
                DeviceID = deviceId,
                CardType = cardRequest.CardType,
                CardName = cardRequest.CardName,
                SerialNumber = cardRequest.SerialNumber
            };
            await cardRepo.CreateAsync(newCard, cancellationToken);
            if (cardRequest.Ports?.Any() == true)
            {
                var newPorts = new List<Rack.Domain.Entities.Port>();
                var portsWithRequestId = new Dictionary<Guid, Rack.Domain.Entities.Port>();
                foreach (var portRequest in cardRequest.Ports)
                {
                    var clientId = portRequest.DeviceID; // Không có Id, dùng DeviceID tạm nếu cần
                    var newPort = new Rack.Domain.Entities.Port
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
        Rack.Domain.Interfaces.IGenericRepository<Rack.Domain.Entities.Port> portRepo,
        CancellationToken cancellationToken)
    {
        var clientPortIdToDbPortId = new Dictionary<Guid, Guid>();
        var newPorts = new List<Rack.Domain.Entities.Port>();
        var portsWithRequestId = new Dictionary<Guid, Rack.Domain.Entities.Port>();
        foreach (var portRequest in portRequests)
        {
            var clientId = portRequest.DeviceID; // Không có Id, dùng DeviceID tạm nếu cần
            var newPort = new Rack.Domain.Entities.Port
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
        Rack.Domain.Interfaces.IGenericRepository<Rack.Domain.Entities.PortConnection> portConnectionRepo,
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