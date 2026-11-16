using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Application.Security;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Commands;

/// <summary>
/// Replaces the user, storing a fresh salted hash of <see cref="Password"/>, and answers the stored user. Throws
/// <see cref="ConflictException"/> when another user already has the email.
/// </summary>
public sealed record UpdateUserCommand(int Id, string FirstName, string LastName, string Email, string Password, int FindeksRating);

public sealed class UpdateUserCommandValidator : UseCaseValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(u => u.FirstName).ValidName();
        RuleFor(u => u.LastName).ValidName();
        RuleFor(u => u.Email).ValidEmail();
        RuleFor(u => u.Password).ValidPassword();
    }
}

public sealed class UpdateUserCommandHandler(IUserDal userDal) : ICommandHandler<UpdateUserCommand, User>
{
    public async Task<User> HandleAsync(UpdateUserCommand command, CancellationToken cancellationToken = default)
    {
        // The unique index refuses it too; asking first turns that into an answer the caller can act on.
        if (await userDal.AnyAsync(u => u.Email == command.Email && u.Id != command.Id, cancellationToken))
        {
            throw new ConflictException(Messages.UserAlreadyExists);
        }

        HashingHelper.CreatePasswordHash(command.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            Id = command.Id,
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Status = true,
            FindeksRating = command.FindeksRating,
        };

        await userDal.UpdateAsync(user, cancellationToken);
        return user;
    }
}
