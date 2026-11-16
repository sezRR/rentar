using System.Collections.Generic;

namespace Rentar.Modules.Companies.Contracts;

/// <param name="UserId">The Identity user employed.</param>
/// <param name="Permissions">Every <see cref="CompanyPermissions"/> value the employee's roles grant.</param>
public sealed record EmployeeSummary(int EmployeeId, int CompanyId, string CompanyName, int UserId, IReadOnlyCollection<string> Permissions);
