using Mapster;
using Rack.Contracts.Role.Responses;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Role.Queries.GetById;

internal class GetRoleByIdQueryHandler(IUnitOfWork unitOfWork) : IQueryHandler<GetRoleByIdQuery, RoleResponse>
{
    public async Task<Response<RoleResponse>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var roleRepository = unitOfWork.GetRepository<Domain.Entities.Role>();
        var role = await roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role is null)
        {
            return Response<RoleResponse>.Failure(
                Error.NotFound(message: "Không tìm thấy role này"),
                Domain.Enum.HttpStatusCodeEnum.NotFound);
        }
        var roleResponse = role.Adapt<RoleResponse>();
        return Response<RoleResponse>.Success(roleResponse);
    }
}