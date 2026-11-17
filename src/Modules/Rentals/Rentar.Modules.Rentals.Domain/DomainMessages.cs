namespace Rentar.Modules.Rentals.Domain;

/// <summary>What the domain says when it refuses something; meant for the caller, like the application's messages.</summary>
public static class DomainMessages
{
    public const string RentalRentDateMissing = "Rental rent date must be set";
    public const string RentalReturnDateNotAfterRentDate = "Rental return date must be after its rent date";

    public const string RentalNotPending = "Only a pending rental can be confirmed";
    public const string RentalAlreadyCancelled = "The rental is cancelled already";
}
