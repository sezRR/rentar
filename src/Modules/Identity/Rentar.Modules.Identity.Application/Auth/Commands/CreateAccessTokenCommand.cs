using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Identity.Application.Auth.Commands;

/// <summary>Issues an access token carrying the user's operation claims.</summary>
public sealed record CreateAccessTokenCommand(User User);

public sealed class CreateAccessTokenCommandHandler(IUserDal userDal, ITokenHelper tokenHelper) : ICommandHandler<CreateAccessTokenCommand, AccessToken>
{
    public AccessToken Handle(CreateAccessTokenCommand command)
    {
        return tokenHelper.CreateToken(command.User, userDal.GetClaims(command.User));
    }
}
