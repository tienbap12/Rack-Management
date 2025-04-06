using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Primitives;

namespace Rack.Application.Feature.DataCenter.Commands.Update;

internal class UpdateDataCenterCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<UpdateDataCenterCommand, Response>
{
    public async Task<Response> Handle(UpdateDataCenterCommand request, CancellationToken cancellationToken)
    {
        var dcRepo = unitOfWork.GetRepository<Rack.Domain.Entities.DataCenter>();
        var existDC = await dcRepo.GetByIdAsync(request.DCId, cancellationToken);
        if (existDC == null)
        {
            return Response.Failure(Error.NotFound("Không tìm thấy DataCenter này"));
        }
        existDC.Name = request.Name;
        existDC.Location = request.Location;
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}