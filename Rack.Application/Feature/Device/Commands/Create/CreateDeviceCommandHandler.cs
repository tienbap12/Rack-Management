using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;
using Rack.Domain.Interfaces;

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
                LinkIdPage = request.LinkPageId,
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

            // --- Tạo các Card cho Device chính ---
            if (request.Cards?.Any() == true)
            {
                var cards = request.Cards
                    .Select(card => new Domain.Entities.Card
                    {
                        DeviceID = newDevice.Id,
                        CardType = card.CardType,
                        CardName = card.CardName
                    });
                await cardRepo.InsertRangeAsync(cards, cancellationToken);
            }

            // --- Tạo các Port cho Device chính ---
            if (request.Ports?.Any() == true)
            {
                var ports = request.Ports
                    .Select(port => new Domain.Entities.Port
                    {
                        DeviceID = newDevice.Id,
                        CardID = port.CardID,
                        PortName = port.PortName,
                        PortType = port.PortType,
                    });
                await portRepo.InsertRangeAsync(ports, cancellationToken);
            }

            // --- Tạo các PortConnection cho Device chính ---
            if (request.PortConnections?.Any() == true)
            {
                var portConnections = request.PortConnections
                    .Select(pc => new Domain.Entities.PortConnection
                    {
                        SourcePortID = pc.SourcePortID,
                        DestinationPortID = pc.DestinationPortID,
                        CableType = pc.CableType,
                        Comment = pc.Comment
                    });
                await portConnectionRepo.InsertRangeAsync(portConnections, cancellationToken);
            }

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
                        var childCards = child.Cards
                            .Select(card => new Domain.Entities.Card
                            {
                                DeviceID = childDevice.Id,
                                CardType = card.CardType,
                                CardName = card.CardName
                            });
                        await cardRepo.InsertRangeAsync(childCards, cancellationToken);
                        needsSave = true;
                    }

                    // Tạo Ports cho Child Device
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

                    // Tạo PortConnections cho Child Device
                    if (child.PortConnections?.Any() == true)
                    {
                        var childPortConnections = child.PortConnections
                            .Select(pc => new Domain.Entities.PortConnection
                            {
                                SourcePortID = pc.SourcePortID,
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
            var result = newDevice.Adapt<DeviceResponse>();
            return Response<DeviceResponse>.Success(result);
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
}