using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using System;

namespace Rack.Application.Feature.User.Queries.GetProfile;

public record GetProfileQuery(string UserId) : IQuery<ProfileResponse>;