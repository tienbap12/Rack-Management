using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;

namespace Rack.Application.Feature.DataCenter.Commands.Create;

internal class CreateDataCenterCommandHandler(IUnitOfWork unitOfWork) : ICommandHandler<CreateDataCenterCommand, Response>
{
    public async Task<Response> Handle(CreateDataCenterCommand request, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);
        var dcRepo = unitOfWork.GetRepository<Domain.Entities.DataCenter>();

        // Kiểm tra trùng lặp
        var existingDC = await dcRepo.BuildQuery
            .Where(x => x.Name == request.Name)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingDC != null)
        {
            return Error.Conflict(
                $"Data Center with name '{request.Name}' already exists"
            );
        }

        // Mapping và tạo mới
        var newDC = request.Adapt<Domain.Entities.DataCenter>();
        await dcRepo.CreateAsync(newDC, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Response.Success();
    }
}