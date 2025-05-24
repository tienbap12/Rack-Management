using System;

namespace Rack.Domain.Commons.Abstractions
{
    /// <summary>
    /// Service for accessing the currently authenticated user's information
    /// </summary>
    public interface ICurrentUserService
    {
        /// <summary>
        /// Gets the ID of the current user
        /// </summary>
        Guid UserId { get; }

        /// <summary>
        /// Gets the username of the current user
        /// </summary>
        string Username { get; }

        /// <summary>
        /// Gets the roles of the current user
        /// </summary>
        string[] Roles { get; }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        /// <param name="role">The role to check</param>
        /// <returns>True if the user has the role, false otherwise</returns>
        bool IsInRole(string role);
    }
}