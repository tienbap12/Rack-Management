using Rack.Contracts.Port.Response;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;

namespace Rack.Application.Feature.Port.Queries.GetAll;

public record GetAllPortQuery(Guid? DeviceId, Guid? CardId) : IQuery<List<PortResponse>>; 