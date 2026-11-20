using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.Data;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Common.Interfaces;

public interface IUserDal : IEntityRepository<User>
{
    /// <summary>The operation claims granted to the user.</summary>
    Task<List<OperationClaim>> GetClaimsAsync(User user, CancellationToken cancellationToken = default);
}
