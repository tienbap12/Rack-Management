using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;

namespace Rack.Application.Feature.Device.Commands.Update;

internal class UpdateDeviceCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateDeviceCommand, Response<DeviceResponse>>
{
    public async Task<Response<DeviceResponse>> Handle(
        UpdateDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var deviceRepository = unitOfWork.GetRepository<Domain.Entities.Device>();
        var deviceRackRepository = unitOfWork.GetRepository<Domain.Entities.DeviceRack>();
        var configItemRepo = unitOfWork.GetRepository<Domain.Entities.ConfigurationItem>();
        var cardRepo = unitOfWork.GetRepository<Card>();
        var portRepo = unitOfWork.GetRepository<Port>();

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
        device.UPosition = request.Request.UPosition ?? device.UPosition;
        device.SlotInParent = request.Request.SlotInParent ?? device.SlotInParent;
        device.DeviceType = request.Request.DeviceType ?? device.DeviceType;
        device.IpAddress = request.Request.IpAddress ?? device.IpAddress;
        device.Name = request.Request.Name ?? device.Name;
        device.Model = request.Request.Model ?? device.Model;
        device.SerialNumber = request.Request.SerialNumber ?? device.SerialNumber;
        device.Size = request.Request.Size ?? device.Size;
        device.RackID = request.Request.RackID ?? device.RackID;
        device.ParentDeviceID = request.Request.ParentDeviceID ?? device.ParentDeviceID;

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
            await unitOfWork.SaveChangesAsync();
        }

        // --- Update Cards ---
        if (request.Request.Cards?.Any() == true)
        {
            var oldCards = cardRepo.BuildQuery.Where(c => c.DeviceID == device.Id);
            await cardRepo.DeleteRangeAsync(oldCards, cancellationToken);

            var newCards = request.Request.Cards
                .Select(card => new Card
                {
                    DeviceID = device.Id,
                    CardType = card.CardType,
                    CardName = card.CardName
                });

            await cardRepo.InsertRangeAsync(newCards, cancellationToken);
            await unitOfWork.SaveChangesAsync();

        }

        // --- Update Ports ---
        if (request.Request.Ports?.Any() == true)
        {
            var oldPorts = portRepo.BuildQuery.Where(p => p.DeviceID == device.Id);
            await portRepo.DeleteRangeAsync(oldPorts, cancellationToken);

            var newPorts = request.Request.Ports
                .Select(port => new Port
                {
                    DeviceID = device.Id,
                    CardID = port.CardID,
                    PortName = port.PortName,
                    PortType = port.PortType
                });

            await portRepo.InsertRangeAsync(newPorts, cancellationToken);
            await unitOfWork.SaveChangesAsync();

        }

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
        await unitOfWork.SaveChangesAsync();
        var result = device.Adapt<DeviceResponse>();
        return Response<DeviceResponse>.Success(result);
    }
}