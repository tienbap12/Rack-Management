using Mapster;
using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.Component.Queries.GetById;

internal class GetComponentByIdQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetComponentByIdQuery, ComponentResponse>
{
    public async Task<Response<ComponentResponse>> Handle(
        GetComponentByIdQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var componentRepository = unitOfWork.GetRepository<Domain.Entities.Component>();

            var component = await componentRepository.GetByIdAsync(request.Id, cancellationToken);

            if (component == null)
            {
                return Response<ComponentResponse>.Failure(Error.NotFound(), Domain.Enum.HttpStatusCodeEnum.NotFound);
            }

            var componentResult = component.Adapt<ComponentResponse>();
            return Response<ComponentResponse>.Success(componentResult);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in GetComponentByIdQueryHandler: {ex}");
            // Log lỗi
            return Response<ComponentResponse>.Failure(Error.Failure(), Domain.Enum.HttpStatusCodeEnum.InternalServerError);
        }
    }
}