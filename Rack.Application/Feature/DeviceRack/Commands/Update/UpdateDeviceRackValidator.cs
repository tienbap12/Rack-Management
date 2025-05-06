using FluentValidation;
using System;

namespace Rack.Application.Feature.DeviceRack.Commands.Update;

public class UpdateDeviceRackValidator : AbstractValidator<UpdateDeviceRackCommand>
{
    public UpdateDeviceRackValidator()
    {
        RuleFor(x => x.RackId)
            .NotEmpty().WithMessage("ID Rack không được để trống");

        When(x => x.RackNumber != null, () => {
            RuleFor(x => x.RackNumber)
                .NotEmpty().WithMessage("Mã Rack không được để trống")
                .MaximumLength(50).WithMessage("Mã Rack không được vượt quá 50 ký tự");
        });

        When(x => x.Size.HasValue, () => {
            RuleFor(x => x.Size.Value)
                .GreaterThan(0).WithMessage("Kích thước Rack phải lớn hơn 0")
                .LessThanOrEqualTo(52).WithMessage("Kích thước Rack không được lớn hơn 52U");
        });

        When(x => x.DataCenterID != Guid.Empty, () => {
            RuleFor(x => x.DataCenterID)
                .NotEmpty().WithMessage("ID trung tâm dữ liệu không được để trống");
        });
    }
} 