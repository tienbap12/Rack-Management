using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Role.Commands.Delete;

public class DeleteRoleCommandHandler(IUnitOfWork _unitOfWork) : ICommandHandler<DeleteRoleCommand, Response>
{
    public async Task<Response> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var roleRepository = _unitOfWork.GetRepository<Domain.Entities.Role>();
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
        {
            return Response.Failure(Error.NotFound());
        }
        var existedUserHaveRole = await roleRepository.BuildQuery.Include(a => a.Accounts).AnyAsync(a => a.Accounts.Any(a => a.RoleId == role.Id), cancellationToken);
        if (existedUserHaveRole)
        {
            return Response.Failure(Error.BadRequest());
        }

        await roleRepository.DeleteAsync(role.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success();
    }
}