using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Role.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Interfaces;

namespace Rack.Application.Feature.Role.Queries.GetAll;

public class GetAllRolesQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetAllRolesQuery, List<RoleResponse>>
{
    public async Task<Response<List<RoleResponse>>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = unitOfWork.GetRepository<Domain.Entities.Role>();
        var roleResponses = await roles.BuildQuery.Select(r => new RoleResponse(
            r.Id,
            r.Name,
            r.Status,
            r.CreatedOn,
            r.LastModifiedOn,
            r.CreatedBy,
            r.LastModifiedBy
        )).ToListAsync();

        return Response<List<RoleResponse>>.Success(roleResponses);
    }
}