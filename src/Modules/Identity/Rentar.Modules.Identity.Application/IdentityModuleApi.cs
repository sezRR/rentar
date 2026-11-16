using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application;

public class IdentityModuleApi(IUserDal userDal) : IIdentityModuleApi
{
    public async Task<IReadOnlyList<UserSummary>> GetUsersAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        var ids = userIds.ToArray();
        var users = await userDal.GetAllAsync(u => ids.Contains(u.Id), cancellationToken);
        return users.ConvertAll(u => new UserSummary(u.Id, u.FirstName, u.LastName));
    }
}
