using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Commands.Delete;

internal class DeleteComponentCommandHandler(IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteComponentCommand, Response>
{
    public async Task<Response> Handle( // Hoặc Task<Response<bool>>
        DeleteComponentCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var componentRepository = unitOfWork.GetRepository<Domain.Entities.Component>();
            var componentToDelete = await componentRepository.GetByIdAsync(command.Id, cancellationToken);

            if (componentToDelete == null)
            {
                // Có thể coi như thành công nếu không tìm thấy để xóa idempotency
                // Hoặc trả về NotFound tùy yêu cầu
                return Response.Failure(Error.NotFound());
            }

            // --- Kiểm tra Logic nghiệp vụ trước khi xóa ---
            if (componentToDelete.Status == ComponentStatus.InUse)
            {
                return Response.Failure(Error.Conflict(message: "Thiết bị đang được sử dụng")); // 409 Conflict
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Response.Success("Xóa thiết bị thành công");
        }
        catch (Exception ex)
        {
            return Response.Failure(Error.Failure());
        }
    }
}