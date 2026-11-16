using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;

namespace Rentar.Modules.Identity.Application.Users.Commands;

/// <summary>
/// Stores a user exactly as given, hash and salt included. Accounts with a password are created by
/// <see cref="Auth.Commands.RegisterUserCommand"/>.
/// </summary>
public sealed record AddUserCommand(
    string FirstName,
    string LastName,
    string Email,
    byte[] PasswordHash,
    byte[] PasswordSalt,
    bool Status,
    int FindeksRating);

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
    public void Handle(AddUserCommand command)
    {
        userDal.Add(new User
        {
            FirstName = command.FirstName,
            LastName = command.LastName,
            Email = command.Email,
            PasswordHash = command.PasswordHash,
            PasswordSalt = command.PasswordSalt,
            Status = command.Status,
            FindeksRating = command.FindeksRating
        });
    }
}
