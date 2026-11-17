namespace Rentar.Modules.Findeks.Domain;

/// <summary>What the domain says when it refuses something; meant for the caller, like the application's messages.</summary>
public static class DomainMessages
{
    public const string FindeksFactorOutOfRange = "A Findeks report factor must be between 0 and its maximum";
    public const string FindeksRatingOutOfRange = "A Findeks rating must be between 0 and 1900";
}
