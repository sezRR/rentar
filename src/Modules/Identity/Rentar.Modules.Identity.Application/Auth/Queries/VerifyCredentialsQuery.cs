using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Application.Security;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Auth.Queries;

/// <summary>
/// Answers the user the email and password belong to. Throws <see cref="InvalidCredentialsException"/> when no user
/// has that email or the password is wrong, and <see cref="ForbiddenException"/> when the user is inactive.
/// </summary>
public sealed record VerifyCredentialsQuery(string Email, string Password);

public sealed class VerifyCredentialsQueryHandler(IUserDal userDal) : IQueryHandler<VerifyCredentialsQuery, User>
{
    public async Task<User> HandleAsync(VerifyCredentialsQuery query, CancellationToken cancellationToken = default)
    {
        var user = await userDal.GetAsync(u => u.Email == query.Email, cancellationToken)
            ?? throw new InvalidCredentialsException(Messages.UserNotFound);

        if (!HashingHelper.VerifyPasswordHash(query.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new InvalidCredentialsException(Messages.PasswordError);
        }

        // Only said once the password is right, so it reveals nothing to someone guessing.
        if (!user.Status)
        {
            throw new ForbiddenException(Messages.UserNotActive);
        }

        return user;
    }
}
