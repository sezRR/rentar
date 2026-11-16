using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Application.Security;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Identity.Application.Auth.Queries;

/// <summary>
/// Answers the user the email and password belong to. Throws <see cref="InvalidCredentialsException"/> when no user
/// has that email or the password is wrong.
/// </summary>
public sealed record VerifyCredentialsQuery(string Email, string Password);

public sealed class VerifyCredentialsQueryHandler(IUserDal userDal) : IQueryHandler<VerifyCredentialsQuery, User>
{
    public User Handle(VerifyCredentialsQuery query)
    {
        var user = userDal.Get(u => u.Email == query.Email)
            ?? throw new InvalidCredentialsException(Messages.UserNotFound);

        if (!HashingHelper.VerifyPasswordHash(query.Password, user.PasswordHash, user.PasswordSalt))
        {
            throw new InvalidCredentialsException(Messages.PasswordError);
        }

        return user;
    }
}
