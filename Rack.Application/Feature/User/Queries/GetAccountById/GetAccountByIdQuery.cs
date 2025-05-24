using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using System;

namespace Rack.Application.Feature.User.Queries.GetAccountById;

public record GetAccountByIdQuery(Guid AccountId) : IQuery<AccountResponse>;