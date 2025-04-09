using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DataCenter.Commands.Delete;

internal class DeleteDataCenterCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<DeleteDataCenterCommand, Response>
{
    public async Task<Response> Handle(DeleteDataCenterCommand request, CancellationToken cancellationToken)
    {
        var dcRepo = unitOfWork.GetRepository<Rack.Domain.Entities.DataCenter>();
        var existDC = await dcRepo.GetByIdAsync(request.DCId, cancellationToken);
        if (existDC == null) return Response.Failure(Error.NotFound("Không tìm thấy DataCenter này"));
        await dcRepo.DeleteAsync(request.DCId, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}