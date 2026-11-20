using Rentar.Modules.Identity.Contracts;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application;

internal static class IdentityUsers
{
    /// <summary>
    /// The user, asked of the Identity module since no foreign key guards the reference; throws
    /// <see cref="BusinessRuleException"/> when Identity does not know the user or holds it as inactive.
    /// </summary>
    public static async Task<UserSummary> RequireActiveUserAsync(
        this IIdentityModuleApi identityModule,
        int userId,
        CancellationToken cancellationToken)
    {
        var user = (await identityModule.GetUsersAsync([userId], cancellationToken)).SingleOrDefault()
            ?? throw new BusinessRuleException(Messages.UserNotFound);

        if (!user.IsActive)
        {
            throw new BusinessRuleException(Messages.UserNotActive);
        }

        return user;
    }
}
