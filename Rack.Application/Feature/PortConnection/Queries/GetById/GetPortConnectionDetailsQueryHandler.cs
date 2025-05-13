using MediatR;
using Microsoft.EntityFrameworkCore;
using Rack.Domain.Data;
using Rack.Contracts.PortConnection.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Entities;
using Rack.Domain.Interfaces;
using Rack.Application.Feature.PortConnection.Queries.GetById;
using Rack.Contracts.Device.Responses;
using Rack.Contracts.Port.Response;
using Microsoft.Extensions.Logging;
using Rack.Domain.Enum;

public class GetPortConnectionDetailsQueryHandler : IRequestHandler<GetPortConnectionDetailsQuery, Response<PortConnectionResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPortConnectionDetailsQueryHandler> _logger;

    public GetPortConnectionDetailsQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPortConnectionDetailsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Response<PortConnectionResponse>> Handle(GetPortConnectionDetailsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting port connection details for ID: {PortConnectionId}", request.portConnectionId);

        var portConnection = await _unitOfWork.GetRepository<Rack.Domain.Entities.PortConnection>()
            .Query
            .Include(pc => pc.SourcePort)
                .ThenInclude(p => p.Device)
            .Include(pc => pc.DestinationPort)
                .ThenInclude(p => p.Device)
                .FirstOrDefaultAsync(pc => pc.Id == request.portConnectionId && !pc.IsDeleted, cancellationToken);

        if (portConnection == null)
        {
                _logger.LogWarning("Port connection not found with ID: {PortConnectionId}", request.portConnectionId);
                return Response<PortConnectionResponse>.Failure(
                    Error.NotFound(
                        code: ErrorCode.GeneralNotFound,
                        message: $"Port connection with ID {request.portConnectionId} not found"
                    )
                );
        }

            _logger.LogInformation("Found port connection. Mapping to response...");

        var response = new PortConnectionResponse
        {
                Id = portConnection.Id,
            SourcePortID = portConnection.SourcePortID,
            DestinationPortID = portConnection.DestinationPortID,
            CableType = portConnection.CableType,
            Comment = portConnection.Comment,
            CreatedBy = portConnection.CreatedBy,
            CreatedOn = portConnection.CreatedOn,
            LastModifiedBy = portConnection.LastModifiedBy,
            LastModifiedOn = portConnection.LastModifiedOn,
            IsDeleted = portConnection.IsDeleted,
            DeletedBy = portConnection.DeletedBy,
                DeletedOn = portConnection.DeletedOn,

                // Map source device and port information
                SourceDevice = portConnection.SourcePort?.Device != null ? new SimpleDeviceDto {
                    Id = portConnection.SourcePort.Device.Id,
                    RackName = portConnection.SourcePort.Device.Rack?.RackNumber,
                    Slot = portConnection.SourcePort.Device.SlotInParent,
                    DeviceName = portConnection.SourcePort.Device.Name
                } : null,

                SourcePort = portConnection.SourcePort != null ? new PortResponse
                {
                    Id = portConnection.SourcePort.Id,
                    DeviceID = portConnection.SourcePort.DeviceID,
                    CardID = portConnection.SourcePort.CardID,
                    PortName = portConnection.SourcePort.PortName,
                    PortType = portConnection.SourcePort.PortType
                } : null,

                // Map destination device and port information
                DestinationDevice = portConnection.DestinationPort?.Device != null ? new SimpleDeviceDto {
                    Id = portConnection.DestinationPort.Device.Id,
                    RackName = portConnection.DestinationPort.Device.Rack?.RackNumber,
                    Slot = portConnection.DestinationPort.Device.SlotInParent,
                    DeviceName = portConnection.DestinationPort.Device.Name
                } : null,

                DestinationPort = portConnection.DestinationPort != null ? new PortResponse
                {
                    Id = portConnection.DestinationPort.Id,
                    DeviceID = portConnection.DestinationPort.DeviceID,
                    CardID = portConnection.DestinationPort.CardID,
                    PortName = portConnection.DestinationPort.PortName,
                    PortType = portConnection.DestinationPort.PortType
                } : null
            };

            _logger.LogInformation("Successfully mapped port connection to response");
        return Response<PortConnectionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting port connection details for ID: {PortConnectionId}", request.portConnectionId);
            return Response<PortConnectionResponse>.Failure(
                Error.Failure(
                    code: ErrorCode.GeneralFailure,
                    message: "An error occurred while retrieving port connection details"
                )
            );
        }
    }
} 