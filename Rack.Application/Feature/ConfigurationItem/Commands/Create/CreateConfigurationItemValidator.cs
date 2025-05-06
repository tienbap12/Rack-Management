using FluentValidation;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Create;

public class CreateConfigurationItemValidator : AbstractValidator<CreateConfigurationItemCommand>
{
    public CreateConfigurationItemValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("ID thiết bị không được để trống");

        RuleFor(x => x.ConfigType)
            .NotEmpty().WithMessage("Loại cấu hình không được để trống")
            .MaximumLength(100).WithMessage("Loại cấu hình không được vượt quá 100 ký tự");

        RuleFor(x => x.Value)
            .NotEmpty().WithMessage("Giá trị cấu hình không được để trống")
            .MaximumLength(500).WithMessage("Giá trị cấu hình không được vượt quá 500 ký tự");
    }
} 