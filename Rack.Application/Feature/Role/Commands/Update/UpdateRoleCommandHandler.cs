using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Microsoft.EntityFrameworkCore;

namespace Rack.Application.Feature.Role.Commands.Update;

public class UpdateRoleCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateRoleCommand, Response>
{
    public async Task<Response> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRepository = unitOfWork.GetRepository<Domain.Entities.Role>();
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            return Response.Failure("Không tìm thấy Role này!", Domain.Enum.HttpStatusCodeEnum.NotFound);
        }

        // Log giá trị trước khi update
        Console.WriteLine($"[Before Update] Role: Id={role.Id}, Name={role.Name}, Status={role.Status}");

        role.Name = request.Name ?? role.Name;
        if (request.Status != role.Status)
            role.Status = request.Status;
        // Sử dụng phương thức SetEntityState mới
        roleRepository.SetEntityState(role, EntityState.Modified);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success("Cập nhật Role thành công!", Domain.Enum.HttpStatusCodeEnum.OK);
    }
}