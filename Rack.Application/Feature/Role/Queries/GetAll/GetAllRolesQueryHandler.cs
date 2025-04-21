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
        var roleResponses = await roles.BuildQuery
            .Select(r => new RoleResponse
            {
                Id = r.Id,
                Name = r.Name,
                Status = r.Status,
                CreatedOn = r.CreatedOn,
                CreatedBy = r.CreatedBy,
                LastModifiedOn = r.LastModifiedOn,
                LastModifiedBy = r.LastModifiedBy,
                IsDeleted = r.IsDeleted,
                DeletedOn = r.DeletedOn,
                DeletedBy = r.DeletedBy
            }).ToListAsync();

        return Response<List<RoleResponse>>.Success(roleResponses);
    }
}