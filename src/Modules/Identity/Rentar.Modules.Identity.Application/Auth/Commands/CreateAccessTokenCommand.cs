using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Auth.Commands;

/// <summary>Issues an access token carrying the user's operation claims.</summary>
public sealed record CreateAccessTokenCommand(User User);

public sealed class CreateAccessTokenCommandHandler(IUserDal userDal, ITokenHelper tokenHelper) : ICommandHandler<CreateAccessTokenCommand, AccessToken>
{
    public async Task<AccessToken> HandleAsync(CreateAccessTokenCommand command, CancellationToken cancellationToken = default)
    {
        var claims = await userDal.GetClaimsAsync(command.User, cancellationToken);
        return tokenHelper.CreateToken(command.User, claims);
    }
}
