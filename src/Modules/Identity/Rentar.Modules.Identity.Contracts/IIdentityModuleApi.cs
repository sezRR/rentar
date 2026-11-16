using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Contracts;

/// <summary>
/// What other modules may ask the Identity module.
/// </summary>
public interface IIdentityModuleApi
{
    /// <summary>Users with the given ids; unknown ids are left out.</summary>
    Task<IReadOnlyList<UserSummary>> GetUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default);
}
