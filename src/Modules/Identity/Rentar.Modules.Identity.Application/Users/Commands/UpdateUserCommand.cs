using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Application.Security;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;

namespace Rentar.Modules.Identity.Application.Users.Commands;

/// <summary>Replaces the user, storing a fresh salted hash of <see cref="Password"/>, and answers the stored user.</summary>
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
    public User Handle(UpdateUserCommand command)
    {
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

        userDal.Update(user);
        return user;
    }
}
