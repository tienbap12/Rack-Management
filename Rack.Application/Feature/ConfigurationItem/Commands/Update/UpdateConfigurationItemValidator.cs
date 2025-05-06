using FluentValidation;

namespace Rack.Application.Feature.ConfigurationItem.Commands.Update;

public class UpdateConfigurationItemValidator : AbstractValidator<UpdateConfigurationItemCommand>
{
    public UpdateConfigurationItemValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID cấu hình không được để trống");

        When(x => x.ConfigType != null, () => {
            RuleFor(x => x.ConfigType)
                .NotEmpty().WithMessage("Loại cấu hình không được để trống")
                .MaximumLength(100).WithMessage("Loại cấu hình không được vượt quá 100 ký tự");
        });

        When(x => x.ConfigValue != null, () => {
            RuleFor(x => x.ConfigValue)
                .NotEmpty().WithMessage("Giá trị cấu hình không được để trống")
                .MaximumLength(500).WithMessage("Giá trị cấu hình không được vượt quá 500 ký tự");
        });
    }
} 