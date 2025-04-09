using FluentValidation;

namespace Rack.Application.Feature.DataCenter.Commands.Update;

public class UpdateDataCenterCommandValidator : AbstractValidator<UpdateDataCenterCommand>
{
    public UpdateDataCenterCommandValidator()
    {
        RuleFor(x => x.DCId)
            .NotEmpty().WithMessage("Id không được để trống");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên không được để trống")
            .MaximumLength(100).WithMessage("Tên không được quá 100 ký tự");

        RuleFor(x => x.Location)
            .MaximumLength(200).WithMessage("Vị trí không được quá 200 ký tự");
    }
}