using System.Collections.Generic;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.Data;

namespace Rentar.Modules.Identity.Application.Common.Interfaces;

public interface IUserDal : IEntityRepository<User>
{
    List<OperationClaim> GetClaims(User user);
}
