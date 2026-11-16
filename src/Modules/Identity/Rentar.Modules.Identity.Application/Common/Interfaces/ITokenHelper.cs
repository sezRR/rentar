using System.Collections.Generic;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Domain;

namespace Rentar.Modules.Identity.Application.Common.Interfaces;

public interface ITokenHelper
{
    AccessToken CreateToken(User user, List<OperationClaim> operationClaims);
}
