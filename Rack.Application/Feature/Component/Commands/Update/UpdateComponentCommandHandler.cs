using Rack.Contracts.Component.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Enum;

namespace Rack.Application.Feature.Component.Commands.Update;

internal class UpdateComponentCommandHandler(IUnitOfWork unitOfWork)
: ICommandHandler<UpdateComponentCommand, Response> // Hoặc ICommandHandler<UpdateComponentCommand> nếu trả về Unit
{
    public async Task<Response> Handle(
        UpdateComponentCommand command,
        CancellationToken cancellationToken)
    {
        // --- Validation cho request (nếu có) ---
        // Ví dụ: không thể vừa set CurrentDeviceId vừa set StorageRackId

        try
        {
            var componentRepository = unitOfWork.GetRepository<Domain.Entities.Component>();
            var componentToUpdate = await componentRepository.GetByIdAsync(command.Id, cancellationToken);
            // Hoặc dùng query:
            // var componentToUpdate = await componentRepository.BuildQuery
            //                             .FirstOrDefaultAsync(c => c.Id == command.Id && !c.IsDeleted, cancellationToken);

            if (componentToUpdate == null || componentToUpdate.IsDeleted)
            {
                return Response.Failure(Error.NotFound(message: "Không tìm thấy thiết bị"));
            }

            // Ví dụ cập nhật thủ công để kiểm soát:
            if (command.Name != null) componentToUpdate.Name = command.Name;
            if (command.Status.HasValue)
            {
                // *** Thêm Logic Nghiệp Vụ Quan Trọng Ở Đây ***
                // Ví dụ: Khi chuyển Status sang InUse, đảm bảo CurrentDeviceId được cung cấp và StorageRackId là null
                if (command.Status.Value == ComponentStatus.InUse)
                {
                    if (!command.CurrentDeviceId.HasValue || command.CurrentDeviceId == Guid.Empty)
                    {
                        return Response<ComponentResponse>.Failure(Error.Validation(message: "Sai định dạng"), HttpStatusCodeEnum.BadRequest);
                    }
                    componentToUpdate.CurrentDeviceID = command.CurrentDeviceId.Value;
                    componentToUpdate.StorageRackID = null; // Gỡ khỏi vị trí lưu trữ
                }
                // Ví dụ: Khi chuyển Status sang Available, đảm bảo CurrentDeviceId là null
                else if (command.Status.Value == ComponentStatus.Available)
                {
                    componentToUpdate.CurrentDeviceID = null;
                    // StorageRackId có thể được cập nhật hoặc giữ nguyên (tùy logic gỡ)
                    if (command.StorageRackId.HasValue)
                        componentToUpdate.StorageRackID = command.StorageRackId;
                }
                // Thêm các xử lý cho trạng thái khác...
                componentToUpdate.Status = command.Status.Value;
            }
            // Cập nhật các trường khác tương tự...
            if (command.StorageRackId.HasValue && componentToUpdate.Status == ComponentStatus.Available)
                componentToUpdate.StorageRackID = command.StorageRackId;
            if (command.CurrentDeviceId.HasValue && componentToUpdate.Status == ComponentStatus.InUse)
                componentToUpdate.CurrentDeviceID = command.CurrentDeviceId;
            // ... Manufacturer, Model, SpecificationDetails ...

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Response.Success("Cập nhật thành công");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdateComponentCommandHandler: {ex}");
            // Log lỗi
            return Response<ComponentResponse>.Failure(Error.Failure(), HttpStatusCodeEnum.InternalServerError);
        }
    }
}