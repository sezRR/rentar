using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Application.Security;
using Rentar.Modules.Identity.Application.Users;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;

namespace Rentar.Modules.Identity.Application.Auth.Commands;

/// <summary>
/// Creates an account with a salted hash of <see cref="Password"/> and answers the stored user. Throws
/// <see cref="ConflictException"/> when the email is already registered.
/// </summary>
public sealed record RegisterUserCommand(string FirstName, string LastName, string Email, string Password, int FindeksRating);

public sealed class RegisterUserCommandValidator : UseCaseValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(u => u.FirstName).ValidName();
        RuleFor(u => u.LastName).ValidName();
        RuleFor(u => u.Email).ValidEmail();
        RuleFor(u => u.Password).ValidPassword();
    }
}

public sealed class RegisterUserCommandHandler(IUserDal userDal) : ICommandHandler<RegisterUserCommand, User>
{
    public User Handle(RegisterUserCommand command)
    {
        if (userDal.Get(u => u.Email == command.Email) != null)
        {
            throw new ConflictException(Messages.UserAlreadyExists);
        }

        HashingHelper.CreatePasswordHash(command.Password, out var passwordHash, out var passwordSalt);

        var user = new User
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            Status = true,
            FindeksRating = command.FindeksRating,
        };

        userDal.Add(user);
        return user;
    }
}
