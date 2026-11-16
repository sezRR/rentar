using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Infrastructure.Persistence;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Infrastructure.Persistence;

public class EfUserDal(IDbContextFactory<IdentityDbContext> contextFactory) : EfEntityRepositoryBase<User, IdentityDbContext>(contextFactory), IUserDal
{
    public async Task<List<OperationClaim>> GetClaimsAsync(User user, CancellationToken cancellationToken = default)
    {
        await using var context = await ContextFactory.CreateDbContextAsync(cancellationToken);

        // The skip navigation reads through the UserOperationClaims join table.
        return await context.Users
            .Where(u => u.Id == user.Id)
            .SelectMany(u => u.OperationClaims)
            .ToListAsync(cancellationToken);
    }
}
