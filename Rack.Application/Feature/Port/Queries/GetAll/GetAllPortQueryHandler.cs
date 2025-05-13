using Mapster;
using Microsoft.EntityFrameworkCore;
using Rack.Contracts.Port.Response;
using Rack.Contracts.PortConnection.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rack.Application.Feature.Port.Queries.GetAll;

internal class GetAllPortQueryHandler(IUnitOfWork unitOfWork)
    : IQueryHandler<GetAllPortQuery, List<PortResponse>>
{
    public async Task<Response<List<PortResponse>>> Handle(
        GetAllPortQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var portRepository = unitOfWork.GetRepository<Domain.Entities.Port>();
            var portConnectionRepository = unitOfWork.GetRepository<Domain.Entities.PortConnection>();

            // Lấy danh sách ports với các điều kiện lọc
            var ports = await portRepository.BuildQuery
                .Where(p => !p.IsDeleted)
                .Where(p => request.DeviceId == null || p.DeviceID == request.DeviceId)
                .Where(p => request.CardId == null || p.CardID == request.CardId)
                .ToListAsync(cancellationToken);

            if (!ports.Any())
            {
                return Response<List<PortResponse>>.Success(new List<PortResponse>());
            }

            // Lấy tất cả PortConnections liên quan
            var portIds = ports.Select(p => p.Id).ToList();
            var portConnections = await portConnectionRepository.BuildQuery
                .Where(pc => portIds.Contains(pc.SourcePortID) || portIds.Contains(pc.DestinationPortID))
                .ToListAsync(cancellationToken);

            // Map sang response
            var portResponses = ports.Select(port =>
            {
                var portResponse = port.Adapt<PortResponse>();
                return portResponse with
                {
                    PortConnections = portConnections
                        .Where(pc => pc.SourcePortID == port.Id || pc.DestinationPortID == port.Id)
                        .Select(pc => pc.Adapt<PortConnectionResponse>())
                        .ToList()
                };
            }).ToList();

            return Response<List<PortResponse>>.Success(portResponses);
        }
        catch (System.Exception ex)
        {
            return Response<List<PortResponse>>.Failure(Error.Failure());
        }
    }
} 