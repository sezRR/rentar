namespace Rentar.Modules.Identity.Domain;

/// <summary>What the domain says when it refuses something; meant for the caller, like the application's messages.</summary>
public static class DomainMessages
{
    public const string UserNameMissing = "A user needs a first and a last name";
    public const string UserEmailInvalid = "A user needs a valid email address";
}
