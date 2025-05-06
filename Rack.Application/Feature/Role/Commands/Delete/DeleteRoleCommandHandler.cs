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
        
        // Sử dụng GetByIdWithTrackingAsync vì cần tracking để xóa
        var role = await roleRepository.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (role == null)
        {
            return Response.Failure(Error.NotFound());
        }

        // Sử dụng ExistsAsync thay vì BuildQuery.Include
        var existedUserHaveRole = await roleRepository.ExistsAsync(
            r => r.Accounts.Any(a => a.RoleId == role.Id), 
            cancellationToken);

        if (existedUserHaveRole)
        {
            return Response.Failure(Error.BadRequest());
        }

        await roleRepository.DeleteAsync(role.Id, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Response.Success();
    }
}