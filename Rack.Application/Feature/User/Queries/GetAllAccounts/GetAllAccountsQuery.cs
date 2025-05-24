using Rack.Contracts.Authentication;
using Rack.Domain.Commons.Primitives;
using System;
using System.Collections.Generic;

namespace Rack.Application.Feature.User.Queries.GetAllAccounts;

public record GetAllAccountsQuery : IQuery<List<AccountResponse>>
{
    public bool IncludeDeleted { get; init; } = false;
}