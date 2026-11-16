namespace Rentar.Modules.Companies.Contracts;

/// <summary>
/// The fixed catalogue of what an employee may do in their company. Roles grant a set of these; code checks them by
/// name, so they are part of the public surface other modules check against (Rentals checks <see cref="RentalsRead"/>).
/// </summary>
/// <remarks>
/// Adding a permission: add it to <see cref="All"/>, and grant it to the stored Owner roles in a migration, since
/// they are copied per company when it registers and never pick up new permissions on their own.
/// </remarks>
public static class CompanyPermissions
{
    /// <summary>Change the company's own details, e.g. its name.</summary>
    public const string CompanyManage = "company.manage";

    /// <summary>List the company's employees and their roles.</summary>
    public const string EmployeesRead = "employees.read";

    /// <summary>Add employees with roles and remove them, within the caller's own permissions.</summary>
    public const string EmployeesManage = "employees.manage";

    /// <summary>See the rentals booked by the company's customers.</summary>
    public const string RentalsRead = "rentals.read";

    public static readonly string[] All = [CompanyManage, EmployeesRead, EmployeesManage, RentalsRead];
}
