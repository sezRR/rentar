using Rentar.Modules.Companies.Domain;
using System.Collections.Generic;

namespace Rentar.Modules.Companies.Application.Common.Models;

/// <summary>
/// An employee with the name of its Identity user and its roles. The names are <c>null</c> when Identity no longer
/// knows the user.
/// </summary>
public sealed record EmployeeResponse(int Id, int CompanyId, int UserId, string FirstName, string LastName, List<Role> Roles);
