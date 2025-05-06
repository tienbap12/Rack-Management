using FluentValidation;
using Rack.Contracts.DeviceRack.Requests;

namespace Rack.Application.Feature.DeviceRack.Commands.Create;

public class CreateDeviceRackValidator : AbstractValidator<CreateDeviceRackCommand>
{
    public CreateDeviceRackValidator()
    {
        RuleFor(x => x.Request.DataCenterID)
            .NotEmpty().WithMessage("ID trung tâm dữ liệu không được để trống");

        RuleFor(x => x.Request.RackNumber)
            .NotEmpty().WithMessage("Mã Rack không được để trống")
            .MaximumLength(50).WithMessage("Mã Rack không được vượt quá 50 ký tự");

        When(x => x.Request.Size.HasValue, () => {
            RuleFor(x => x.Request.Size.Value)
                .GreaterThan(0).WithMessage("Kích thước Rack phải lớn hơn 0")
                .LessThanOrEqualTo(52).WithMessage("Kích thước Rack không được lớn hơn 52U");
        });
    }
} 