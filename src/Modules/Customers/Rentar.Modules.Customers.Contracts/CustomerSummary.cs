namespace Rentar.Modules.Customers.Contracts;

/// <param name="UserId">The Identity user this customer account belongs to.</param>
/// <param name="CompanyName">Free text, <c>null</c> when the customer gave none.</param>
/// <param name="CompanyId">The Companies company the customer rents for, or <c>null</c>.</param>
public sealed record CustomerSummary(int CustomerId, int UserId, string CompanyName, int? CompanyId);
