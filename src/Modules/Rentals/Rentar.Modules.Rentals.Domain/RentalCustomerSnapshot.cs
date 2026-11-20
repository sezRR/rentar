namespace Rentar.Modules.Rentals.Domain;

/// <summary>
/// The customer's user and company as they were when the rental was booked. Later changes, such as a renamed user or
/// company, or the customer leaving the company, leave it as it was.
/// </summary>
/// <param name="CompanyId">The company the customer rented for, or <c>null</c> for a customer with no company.</param>
/// <param name="CompanyName">That company's name, or <c>null</c> for a customer with no company.</param>
public sealed record RentalCustomerSnapshot(
    string FirstName,
    string LastName,
    int? CompanyId,
    string? CompanyName);
