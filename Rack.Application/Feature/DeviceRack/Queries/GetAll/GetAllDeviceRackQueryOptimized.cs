using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Rack.Application.Commons.Helpers;
using Rack.Contracts.DeviceRack.Response;
using Rack.Domain.Commons.Primitives;
using Rack.Domain.Data;
using Rack.Domain.Entities;
using Rack.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Rack.Contracts.Device.Responses;

namespace Rack.Application.Feature.DeviceRack.Queries.GetAll
{
    public record GetAllDeviceRackQueryOptimized : IQuery<IReadOnlyList<DeviceRackQuickResponse>>;

    public class GetAllDeviceRackQueryOptimizedHandler : IQueryHandler<GetAllDeviceRackQueryOptimized, IReadOnlyList<DeviceRackQuickResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GetAllDeviceRackQueryOptimizedHandler> _logger;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(5);
        private const string CacheKey = "device_racks_all";

        public GetAllDeviceRackQueryOptimizedHandler(IUnitOfWork unitOfWork, IMemoryCache cache, ILogger<GetAllDeviceRackQueryOptimizedHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _logger = logger;
        }

        public async Task<Response<IReadOnlyList<DeviceRackQuickResponse>>> Handle(GetAllDeviceRackQueryOptimized request, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _cache.GetOrCreateAsync(
                    CacheKey,
                    async (ct) =>
                    {
                        _logger.LogInformation("Cache miss for device racks list - fetching from database");

                        var rackDtos = await _unitOfWork.GetRepository<Domain.Entities.DeviceRack>()
                            .BuildQuery
                            .AsNoTracking()
                            .Include(r => r.DataCenter)
                            .Include(r => r.Devices)
                            .Select(r => new DeviceRackQuickResponse
                            {
                                Id = r.Id,
                                RackNumber = r.RackNumber,
                                Size = r.Size,
                                DataCenterName = r.DataCenter.Name,
                                DataCenterLocation = r.DataCenter.Location,
                                Devices = r.Devices
                                    .Where(d => !d.IsDeleted && d.Status == DeviceStatus.Active)
                                    .Select(d => new DeviceQuickResponse
                                    {
                                        Id = d.Id,
                                        Name = d.Name,
                                        Size = d.Size,
                                        DeviceType = d.DeviceType,
                                        Status = d.Status.ToString(),
                                        UPosition = d.UPosition,
                                        IpAddress = d.IpAddress,
                                        // Thêm trường cần thiết khác nếu UI cần
                                    }).ToList()
                            })
                            .ToListAsync(ct);

                        return rackDtos;
                    },
                    _cacheExpiration,
                    CacheItemPriority.Normal,
                    cancellationToken);

                return Response<IReadOnlyList<DeviceRackQuickResponse>>.Success(result, "DeviceRacks fetched successfully", HttpStatusCodeEnum.OK);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching device racks");
                return Response<IReadOnlyList<DeviceRackQuickResponse>>.Failure(
                    new Error(ErrorCode.GeneralFailure, ex.Message, ErrorType.Failure),
                    HttpStatusCodeEnum.InternalServerError);
            }
        }
    }
}