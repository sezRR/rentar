namespace Rentar.Modules.Rentals.Application;

public static class Messages
{
    public const string RentalsListed = "Rentals Listed";
    public const string RentalAdded = "Rental Added";
    public const string RentalUpdated = "Rental Updated";
    public const string RentalDeleted = "Rental Deleted";
    public const string CarIsInvalid = "Car is Invalid";
    public const string CarNotAvailable = "This car already rentaled by someone";
    public const string CustomerNotAvailable = "Customer already has a rental during this period.";
    public const string CarNotFound = "Car not found";
    public const string CustomerNotFound = "Customer not found";

    /// <summary>Worded like the v2 not-found problems, so an outsider can not tell a company exists.</summary>
    public static string CompanyNotFound(int companyId) => $"Company {companyId} was not found.";

    public static string PermissionMissing(string permission) => $"Your roles in the company do not grant {permission}";
}
