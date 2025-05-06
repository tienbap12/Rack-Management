using FluentValidation;
using Rack.Application.Feature.Device.Commands.Create;
using Rack.Contracts.Card.Request;
using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Contracts.Device.Requests;
using Rack.Contracts.Port.Request;
using Rack.Contracts.PortConnection.Request;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Commands.Create;

public class CreateDeviceValidator : AbstractValidator<CreateDeviceCommand>
{
    public CreateDeviceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên thiết bị không được để trống")
            .MaximumLength(200).WithMessage("Tên thiết bị không được vượt quá 200 ký tự");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("Kích thước thiết bị phải lớn hơn 0");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("Loại thiết bị không được để trống")
            .MaximumLength(100).WithMessage("Loại thiết bị không được vượt quá 100 ký tự");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái thiết bị không hợp lệ");

        RuleFor(x => x.IpAddress)
            .MaximumLength(50).WithMessage("Địa chỉ IP không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.IpAddress));

        RuleFor(x => x.Manufacturer)
            .MaximumLength(100).WithMessage("Nhà sản xuất không được vượt quá 100 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Manufacturer));

        RuleFor(x => x.SerialNumber)
            .MaximumLength(50).WithMessage("Số serial không được vượt quá 50 ký tự")
            .When(x => !string.IsNullOrEmpty(x.SerialNumber));

        RuleFor(x => x.Model)
            .MaximumLength(100).WithMessage("Model không được vượt quá 100 ký tự")
            .When(x => !string.IsNullOrEmpty(x.Model));

        // Kiểm tra điều kiện logic của thiết bị
        RuleFor(x => x)
            .Must(x => !(x.ParentDeviceID.HasValue && x.RackID.HasValue))
            .WithMessage("Thiết bị không thể đồng thời thuộc về một Rack và một thiết bị cha");
            
        // Validate child devices
        When(x => x.ChildDevices != null && x.ChildDevices.Any(), () => {
            RuleForEach(x => x.ChildDevices)
                .SetValidator(new CreateChildDeviceValidator());
        });
        
        // Validate cards
        When(x => x.Cards != null && x.Cards.Any(), () => {
            RuleForEach(x => x.Cards)
                .SetValidator(new CreateCardValidator());
        });
        
        // Validate ports
        When(x => x.Ports != null && x.Ports.Any(), () => {
            RuleForEach(x => x.Ports)
                .SetValidator(new CreatePortValidator());
        });
        
        // Validate port connections
        When(x => x.PortConnections != null && x.PortConnections.Any(), () => {
            RuleForEach(x => x.PortConnections)
                .SetValidator(new CreatePortConnectionValidator());
        });
        
        // Validate configuration items
        When(x => x.ConfigurationItems != null && x.ConfigurationItems.Any(), () => {
            RuleForEach(x => x.ConfigurationItems)
                .SetValidator(new CreateConfigurationItemValidator());
        });
    }
}

public class CreateChildDeviceValidator : AbstractValidator<CreateChildDeviceRequest>
{
    public CreateChildDeviceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên thiết bị con không được để trống")
            .MaximumLength(200).WithMessage("Tên thiết bị con không được vượt quá 200 ký tự");

        RuleFor(x => x.DeviceType)
            .NotEmpty().WithMessage("Loại thiết bị con không được để trống")
            .MaximumLength(100).WithMessage("Loại thiết bị con không được vượt quá 100 ký tự");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Trạng thái thiết bị con không hợp lệ");

        RuleFor(x => x.SlotInParent)
            .NotEmpty().WithMessage("Slot trong thiết bị cha không được để trống")
            .MaximumLength(50).WithMessage("Slot không được vượt quá 50 ký tự");
    }
}

public class CreateCardValidator : AbstractValidator<CreateCardRequest>
{
    public CreateCardValidator()
    {
        RuleFor(x => x.CardType)
            .NotEmpty().WithMessage("Loại card không được để trống")
            .MaximumLength(100).WithMessage("Loại card không được vượt quá 100 ký tự");

        RuleFor(x => x.CardName)
            .NotEmpty().WithMessage("Tên card không được để trống")
            .MaximumLength(100).WithMessage("Tên card không được vượt quá 100 ký tự");
    }
}

public class CreatePortValidator : AbstractValidator<CreatePortRequest>
{
    public CreatePortValidator()
    {
        RuleFor(x => x.PortName)
            .NotEmpty().WithMessage("Tên port không được để trống")
            .MaximumLength(100).WithMessage("Tên port không được vượt quá 100 ký tự");

        RuleFor(x => x.PortType)
            .NotEmpty().WithMessage("Loại port không được để trống")
            .MaximumLength(50).WithMessage("Loại port không được vượt quá 50 ký tự");
    }
}

public class CreatePortConnectionValidator : AbstractValidator<CreatePortConnectionRequest>
{
    public CreatePortConnectionValidator()
    {
        RuleFor(x => x.SourcePortID)
            .NotEmpty().WithMessage("ID port nguồn không được để trống");

        RuleFor(x => x.DestinationPortID)
            .NotEmpty().WithMessage("ID port đích không được để trống");

        RuleFor(x => x)
            .Must(x => x.SourcePortID != x.DestinationPortID)
            .WithMessage("Port nguồn và port đích không được trùng nhau");
    }
}

public class CreateConfigurationItemValidator : AbstractValidator<CreateConfigurationItemRequest>
{
    public CreateConfigurationItemValidator()
    {
        RuleFor(x => x.ConfigType)
            .NotEmpty().WithMessage("Loại cấu hình không được để trống")
            .MaximumLength(100).WithMessage("Loại cấu hình không được vượt quá 100 ký tự");

        RuleFor(x => x.ConfigValue)
            .NotEmpty().WithMessage("Giá trị cấu hình không được để trống");
    }
} 