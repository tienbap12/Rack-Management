using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DataCenter.Commands.Create;

internal class CreateDataCenterCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateDataCenterCommand, Response>
{
    public async Task<Response> Handle(CreateDataCenterCommand request, CancellationToken cancellationToken)
    {
        // Kiểm tra trùng lặp
        var existingDC = await unitOfWork.GetRepository<Domain.Entities.DataCenter>().BuildQuery
            .Where(x => x.Name == request.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingDC != null)
        {
            return Error.Conflict(
            );
        }

        // Mapping và tạo mới
        var newDC = request.Adapt<Domain.Entities.DataCenter>();
        await unitOfWork.GetRepository<Domain.Entities.DataCenter>().CreateAsync(newDC, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}