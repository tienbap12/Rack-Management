using Mapster;
using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Commands.Create;

internal class CreateComponentCommandHandler(IUnitOfWork unitOfWork)
: ICommandHandler<CreateComponentCommand, Response>
{
    public async Task<Response> Handle(
        CreateComponentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var componentRepository = unitOfWork.GetRepository<Domain.Entities.Component>();

            // Map từ Request DTO sang Entity
            var newComponent = command.Adapt<Domain.Entities.Component>();

            await componentRepository.CreateAsync(newComponent, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            // Map từ Entity đã lưu sang Response DTO
            var componentResult = newComponent.Adapt<ComponentResponse>();

            return Response.Success("Tạo thành công"); // Trả về 201 Created
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in CreateComponentCommandHandler: {ex}");
            // Log lỗi
            return Response<ComponentResponse>.Failure(Error.Failure(), HttpStatusCodeEnum.InternalServerError);
        }
    }
}