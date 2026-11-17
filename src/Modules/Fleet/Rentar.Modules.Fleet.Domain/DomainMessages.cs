namespace Rentar.Modules.Fleet.Domain;

/// <summary>What the domain says when it refuses something; meant for the caller, like the application's messages.</summary>
public static class DomainMessages
{
    public const string BrandNameTooShort = "Brand name must be at least 2 characters";

    /// <summary>v1 has always answered a color update with a short name with this message; V1ContractTests pins it.</summary>
    public const string ColorNameInvalid = "Color Name is Invalid";

    public const string CarDescriptionTooShort = "Car description must be at least 2 characters";
    public const string CarModelYearInvalid = "Car model year must be a positive year";
    public const string CarDailyPriceInvalid = "Car daily price must be above zero";

    public const string CarImageLimitReached = "You can not add more picture about your car. You reached limit of the car images (can only upload 5 images to a car)";
}
