using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Role.Commands.Create;

public class CreateRoleCommandHandler(IUnitOfWork _unitOfWork) : ICommandHandler<CreateRoleCommand, Response>
{
    public async Task<Response> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.GetRepository<Domain.Entities.Role>();
        var existingRole = await roleRepository.BuildQuery.Where(x => x.Name == request.Name).FirstOrDefaultAsync(cancellationToken);
        if (existingRole != null)
            return Response.Failure("Quyền này đã tồn tại!");
        var role = request.Adapt<Domain.Entities.Role>();

        await roleRepository.CreateAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success("Thêm quyền thành công");
    }
}