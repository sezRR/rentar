using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Commands;

/// <summary>
/// Stores a user exactly as given, hash and salt included; throws <see cref="ConflictException"/> when the email is
/// already registered. Accounts with a password are created by <see cref="Auth.Commands.RegisterUserCommand"/>.
/// </summary>
public sealed record AddUserCommand(
    string FirstName,
    string LastName,
    string Email,
    byte[] PasswordHash,
    byte[] PasswordSalt,
    bool Status);

public sealed class AddUserCommandValidator : UseCaseValidator<AddUserCommand>
{
    public AddUserCommandValidator()
    {
        RuleFor(u => u.FirstName).ValidName();
        RuleFor(u => u.LastName).ValidName();
        RuleFor(u => u.Email).ValidEmail();
    }
}

public sealed class AddUserCommandHandler(IUserDal userDal) : ICommandHandler<AddUserCommand>
{
    public async Task HandleAsync(AddUserCommand command, CancellationToken cancellationToken = default)
    {
        // The unique index refuses it too; asking first turns that into an answer the caller can act on.
        if (await userDal.AnyAsync(u => u.Email == command.Email, cancellationToken))
        {
            throw new ConflictException(Messages.UserAlreadyExists);
        }

        var user = new User(
            command.FirstName, command.LastName, command.Email, command.PasswordHash, command.PasswordSalt, command.Status);

        await userDal.AddAsync(user, cancellationToken);
    }
}
