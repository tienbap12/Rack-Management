using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Device.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

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

        // --- Kiểm tra Parent Device và Rack ---
        if (request.ParentDeviceID.HasValue)
        {
            var parentDevice = await deviceRepository.GetByIdAsync(request.ParentDeviceID.Value, cancellationToken);
            if (parentDevice == null)
                return Response<DeviceResponse>.Failure(Error.NotFound(message: "Không tìm thấy thiết bị chứa"));

            if (request.RackID.HasValue || request.UPosition.HasValue)
                return Response<DeviceResponse>.Failure(Error.Validation(message: "A device with a Parent cannot be directly assigned to a Rack/UPosition."), HttpStatusCodeEnum.BadRequest);
        }

        if (request.RackID.HasValue)
        {
            var rackExists = await deviceRackRepository.BuildQuery.Where(r => r.Id == request.RackID.Value && !r.IsDeleted).FirstOrDefaultAsync(cancellationToken);
            if (rackExists == null)
                return Response<DeviceResponse>.Failure(Error.Validation(message: "Specified Rack does not exist."), HttpStatusCodeEnum.BadRequest);

            //if (!request.UPosition.HasValue || request.UPosition.Value < 1)
            //    return Response<DeviceResponse>.Failure(Error.Validation(message: "A valid UPosition is required when assigning to a Rack."), HttpStatusCodeEnum.BadRequest);
        }
        //else if (request.UPosition.HasValue)
        //{
        //    return Response<DeviceResponse>.Failure(Error.Validation(message: "Cannot assign UPosition without a RackID."), HttpStatusCodeEnum.BadRequest);
        //}

        await unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var newDevice = request.Adapt<Domain.Entities.Device>();

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
                        ConfigValue = ci.ConfigValue
                    });
                await configItemRepo.InsertRangeAsync(configItems, cancellationToken);
                await unitOfWork.SaveChangesAsync();

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
                        PortType = port.PortType
                    });
                await portRepo.InsertRangeAsync(ports, cancellationToken);
                await unitOfWork.SaveChangesAsync();

            }

            // --- 🔥 Tạo Child Devices nếu có ---
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

                    // Nếu Child Device có ConfigurationItem/Card/Port thì tạo tiếp:
                    if (child.ConfigurationItems?.Any() == true)
                    {
                        var childConfigItems = child.ConfigurationItems
                            .Select(ci => new Domain.Entities.ConfigurationItem
                            {
                                DeviceID = childDevice.Id,
                                ConfigType = ci.ConfigType,
                                ConfigValue = ci.ConfigValue
                            });
                        await configItemRepo.InsertRangeAsync(childConfigItems, cancellationToken);
                        await unitOfWork.SaveChangesAsync();

                    }

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
                        await unitOfWork.SaveChangesAsync();

                    }

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
                        await unitOfWork.SaveChangesAsync();

                    }
                }
            }
            await unitOfWork.SaveChangesAsync();
            var result = newDevice.Adapt<DeviceResponse>();
            return Response<DeviceResponse>.Success(result);
        }
        catch (Exception ex)
        {
            return Response<DeviceResponse>.Failure(Error.Failure(message: $"An error occurred while creating the device: {ex.Message}"), HttpStatusCodeEnum.InternalServerError);
        }
    }
}