using Rentar.Shared.Kernel.Entities;
using Rentar.Shared.Kernel.Exceptions;
using System.Collections.Generic;

namespace Rentar.Modules.Identity.Domain;

/// <summary>
/// A user account. It is only built with a first and last name and an email address; the constructor refuses
/// others with <see cref="BusinessRuleException"/>.
/// </summary>
public class User : IEntity
{
    /// <summary>For EF Core, which sets the stored values itself when it reads a user.</summary>
    private User()
    {
    }

    public User(string firstName, string lastName, string email, byte[] passwordHash, byte[] passwordSalt, bool status)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
        {
            throw new BusinessRuleException(DomainMessages.UserNameMissing);
        }

        if (!IsEmailAddress(email))
        {
            throw new BusinessRuleException(DomainMessages.UserEmailInvalid);
        }

        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        PasswordSalt = passwordSalt;
        Status = status;
    }

    public int Id { get; private init; }
    public string FirstName { get; private init; }
    public string LastName { get; private init; }

    /// <summary>Unique: no two users share an email.</summary>
    public string Email { get; private init; }

    public byte[] PasswordHash { get; private init; }
    public byte[] PasswordSalt { get; private init; }

    /// <summary>Whether the user is active, so they can sign in.</summary>
    public bool Status { get; private init; }

    /// <summary>The operation claims granted to the user, linked through <see cref="UserOperationClaim"/>.</summary>
    public ICollection<OperationClaim> OperationClaims { get; private init; } = [];

    /// <summary>The same user with new details and password; whether they are active stays as it is.</summary>
    public User WithDetails(string firstName, string lastName, string email, byte[] passwordHash, byte[] passwordSalt)
    {
        return new User(firstName, lastName, email, passwordHash, passwordSalt, Status) { Id = Id };
    }

    /// <summary>The same user, inactive, so they can no longer sign in.</summary>
    public User Deactivated()
    {
        // Copied as stored rather than rebuilt: only the status changes, so nothing else is checked again.
        return new User
        {
            Id = Id,
            FirstName = FirstName,
            LastName = LastName,
            Email = Email,
            PasswordHash = PasswordHash,
            PasswordSalt = PasswordSalt,
            Status = false
        };
    }

    /// <summary>
    /// One <c>@</c>, neither first nor last: the check FluentValidation's <c>EmailAddress()</c> makes, so the domain
    /// and the use cases' validators agree on what an address is.
    /// </summary>
    private static bool IsEmailAddress(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        var at = email.IndexOf('@');
        return at > 0 && at != email.Length - 1 && at == email.LastIndexOf('@');
    }
}
