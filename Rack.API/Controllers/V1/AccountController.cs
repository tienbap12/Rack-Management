using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rack.API.Contracts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;

// Import the correct command and query types
using Rack.Application.Feature.User.Commands.CreateAccount;
using Rack.Application.Feature.User.Commands.DeleteAccount;
using Rack.Application.Feature.User.Commands.UpdateAccount;
using Rack.Application.Feature.User.Queries.GetProfile;
using Rack.Application.Feature.User.Queries.GetAllAccounts;
using Rack.Application.Feature.User.Queries.GetAccountById;
using Rack.Contracts.Authentication;

namespace Rack.API.Controllers.V1;

/// <summary>
/// Controller for managing user accounts.
/// Requires authentication for all endpoints, and Admin role for most operations.
/// </summary>
[Authorize]
public class AccountController : ApiController
{
    /// <summary>
    /// Gets the profile information of the currently authenticated user
    /// </summary>
    /// <returns>Profile information including role</returns>
    [HttpGet(ApiRoutesV1.Account.GetProfile)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var result = await Mediator.Send(new GetProfileQuery(userId));
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a list of all user accounts in the system
    /// </summary>
    /// <param name="includeDeleted">Whether to include soft-deleted accounts in the results</param>
    /// <returns>List of all user accounts</returns>
    [HttpGet(ApiRoutesV1.Account.GetAll)]
    // [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllAccounts([FromQuery] bool includeDeleted = false)
    {
        var query = new GetAllAccountsQuery { IncludeDeleted = includeDeleted };
        var result = await Mediator.Send(query);
        return ToActionResult(result);
    }

    /// <summary>
    /// Gets a specific user account by ID
    /// </summary>
    /// <param name="accountId">The ID of the account to retrieve</param>
    /// <returns>The requested account if found</returns>
    [HttpGet(ApiRoutesV1.Account.GetById)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAccountById([FromRoute] Guid accountId)
    {
        var result = await Mediator.Send(new GetAccountByIdQuery(accountId));
        return ToActionResult(result);
    }

    /// <summary>
    /// Creates a new user account
    /// </summary>
    /// <param name="request">The account creation details</param>
    /// <returns>The newly created account information</returns>
    [HttpPost(ApiRoutesV1.Account.Create)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        var result = await Mediator.Send(new CreateAccountCommand(request));

        if (result.IsSuccess)
        {
            return CreatedAtAction(
                nameof(GetAccountById),
                new { accountId = result.Data.Id },
                result
            );
        }

        return ToActionResult(result);
    }

    /// <summary>
    /// Updates an existing user account
    /// </summary>
    /// <param name="accountId">The ID of the account to update</param>
    /// <param name="request">The account update details</param>
    /// <returns>The updated account information</returns>
    [HttpPut(ApiRoutesV1.Account.Update)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateAccount([FromRoute] Guid accountId, [FromBody] UpdateAccountRequest request)
    {
        var result = await Mediator.Send(new UpdateAccountCommand(accountId, request));
        return ToActionResult(result);
    }

    /// <summary>
    /// Soft-deletes an existing user account
    /// </summary>
    /// <param name="accountId">The ID of the account to delete</param>
    /// <returns>Success indicator</returns>
    [HttpDelete(ApiRoutesV1.Account.Delete)]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteAccount([FromRoute] Guid accountId)
    {
        var result = await Mediator.Send(new DeleteAccountCommand(accountId));

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return ToActionResult(result);
    }
}