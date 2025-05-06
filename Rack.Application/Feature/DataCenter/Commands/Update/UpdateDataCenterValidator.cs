using FluentValidation;

namespace Rack.Application.Feature.DataCenter.Commands.Update;

public class UpdateDataCenterValidator : AbstractValidator<UpdateDataCenterCommand>
{
    public UpdateDataCenterValidator()
    {
        RuleFor(x => x.DCId)
            .NotEmpty().WithMessage("ID trung tâm dữ liệu không được để trống");

        When(x => x.Name != null, () => {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên trung tâm dữ liệu không được để trống")
                .MaximumLength(100).WithMessage("Tên trung tâm dữ liệu không được vượt quá 100 ký tự");
        });

        When(x => x.Location != null, () => {
            RuleFor(x => x.Location)
                .MaximumLength(200).WithMessage("Địa điểm không được vượt quá 200 ký tự");
        });
    }
} 