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
            return Response.Failure(Error.NotFound("Không tìm thấy Role này!"));
        }

        role.Name = request.Name;
        role.Status = request.Status;
        role.LastModifiedBy = request.LastModifiedBy;
        role.LastModifiedOn = DateTime.UtcNow;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success("Cập nhật Role thành công!");
    }
}