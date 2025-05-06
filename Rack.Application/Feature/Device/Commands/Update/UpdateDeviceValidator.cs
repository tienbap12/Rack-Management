using FluentValidation;
using Rack.Contracts.Card.Request;
using Rack.Contracts.ConfigurationItem.Requests;
using Rack.Contracts.Device.Requests;
using Rack.Contracts.Port.Request;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Device.Commands.Update;

public class UpdateDeviceValidator : AbstractValidator<UpdateDeviceCommand>
{
    public UpdateDeviceValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("ID thiết bị không được để trống");

        When(x => x.Request.Name != null, () => {
            RuleFor(x => x.Request.Name)
                .NotEmpty().WithMessage("Tên thiết bị không được để trống")
                .MaximumLength(200).WithMessage("Tên thiết bị không được vượt quá 200 ký tự");
        });

        When(x => x.Request.Size.HasValue, () => {
            RuleFor(x => x.Request.Size.Value)
                .GreaterThan(0).WithMessage("Kích thước thiết bị phải lớn hơn 0");
        });

        When(x => x.Request.DeviceType != null, () => {
            RuleFor(x => x.Request.DeviceType)
                .NotEmpty().WithMessage("Loại thiết bị không được để trống")
                .MaximumLength(100).WithMessage("Loại thiết bị không được vượt quá 100 ký tự");
        });

        When(x => x.Request.Status.HasValue, () => {
            RuleFor(x => x.Request.Status.Value)
                .IsInEnum().WithMessage("Trạng thái thiết bị không hợp lệ");
        });

        When(x => x.Request.IpAddress != null, () => {
            RuleFor(x => x.Request.IpAddress)
                .MaximumLength(50).WithMessage("Địa chỉ IP không được vượt quá 50 ký tự");
        });

        When(x => x.Request.Manufacturer != null, () => {
            RuleFor(x => x.Request.Manufacturer)
                .MaximumLength(100).WithMessage("Nhà sản xuất không được vượt quá 100 ký tự");
        });

        When(x => x.Request.SerialNumber != null, () => {
            RuleFor(x => x.Request.SerialNumber)
                .MaximumLength(50).WithMessage("Số serial không được vượt quá 50 ký tự");
        });

        When(x => x.Request.Model != null, () => {
            RuleFor(x => x.Request.Model)
                .MaximumLength(100).WithMessage("Model không được vượt quá 100 ký tự");
        });

        // Kiểm tra điều kiện logic của thiết bị
        RuleFor(x => x.Request)
            .Must(x => !(x.ParentDeviceID.HasValue && x.RackID.HasValue))
            .WithMessage("Thiết bị không thể đồng thời thuộc về một Rack và một thiết bị cha")
            .When(x => x.Request.ParentDeviceID.HasValue || x.Request.RackID.HasValue);

        // Validate child devices nếu có
        When(x => x.Request.ChildDevices != null && x.Request.ChildDevices.Any(), () => {
            RuleForEach(x => x.Request.ChildDevices)
                .SetValidator(new UpdateChildDeviceValidator());
        });

        // Validate cards nếu có
        When(x => x.Request.Cards != null && x.Request.Cards.Any(), () => {
            RuleForEach(x => x.Request.Cards)
                .SetValidator(new UpdateCardValidator());
        });

        // Validate ports nếu có
        When(x => x.Request.Ports != null && x.Request.Ports.Any(), () => {
            RuleForEach(x => x.Request.Ports)
                .SetValidator(new UpdatePortValidator());
        });

        // Validate configuration items nếu có
        When(x => x.Request.ConfigurationItems != null && x.Request.ConfigurationItems.Any(), () => {
            RuleForEach(x => x.Request.ConfigurationItems)
                .SetValidator(new UpdateConfigurationItemValidator());
        });
    }
}

public class UpdateChildDeviceValidator : AbstractValidator<CreateChildDeviceRequest>
{
    public UpdateChildDeviceValidator()
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

public class UpdateCardValidator : AbstractValidator<CreateCardRequest>
{
    public UpdateCardValidator()
    {
        RuleFor(x => x.CardType)
            .NotEmpty().WithMessage("Loại card không được để trống")
            .MaximumLength(100).WithMessage("Loại card không được vượt quá 100 ký tự");

        RuleFor(x => x.CardName)
            .NotEmpty().WithMessage("Tên card không được để trống")
            .MaximumLength(100).WithMessage("Tên card không được vượt quá 100 ký tự");
    }
}

public class UpdatePortValidator : AbstractValidator<CreatePortRequest>
{
    public UpdatePortValidator()
    {
        RuleFor(x => x.PortName)
            .NotEmpty().WithMessage("Tên port không được để trống")
            .MaximumLength(100).WithMessage("Tên port không được vượt quá 100 ký tự");

        RuleFor(x => x.PortType)
            .NotEmpty().WithMessage("Loại port không được để trống")
            .MaximumLength(50).WithMessage("Loại port không được vượt quá 50 ký tự");
    }
}

public class UpdateConfigurationItemValidator : AbstractValidator<CreateConfigurationItemRequest>
{
    public UpdateConfigurationItemValidator()
    {
        RuleFor(x => x.ConfigType)
            .NotEmpty().WithMessage("Loại cấu hình không được để trống")
            .MaximumLength(100).WithMessage("Loại cấu hình không được vượt quá 100 ký tự");

        RuleFor(x => x.ConfigValue)
            .NotEmpty().WithMessage("Giá trị cấu hình không được để trống");
    }
} 