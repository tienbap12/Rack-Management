using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Application.Primitives;
using Rack.Doamin.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DataCenter.Commands.Create;

internal class CreateDataCenterCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateDataCenterCommand, Response>
{
    public async Task<Response> Handle(CreateDataCenterCommand request, CancellationToken cancellationToken)
    {
        var dcRepo = unitOfWork.GetRepository<Domain.Entities.DataCenter>();
        var existingDC = await dcRepo.BuildQuery.Where(x => x.Name == request.Name).FirstOrDefaultAsync(cancellationToken);
        if (existingDC != null)
        {
            return Response.Failure("Data Center đã tồn tại!");
        }
        var newDC = request.Adapt<Domain.Entities.DataCenter>();
        await dcRepo.CreateAsync(newDC, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.CreateSuccessfully("Tạo Data Center thành công!");
    }
}
