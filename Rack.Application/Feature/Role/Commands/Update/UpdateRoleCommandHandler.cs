using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

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

        role.Name = request.Name;
        role.Status = request.Status;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success("Cập nhật Role thành công!", Domain.Enum.HttpStatusCodeEnum.OK);
    }
}