using System.Collections.Generic;

namespace Rentar.Api.IntegrationTests;

/// <summary>An employee as the API sends it; the application's <c>EmployeeBody</c> holds domain roles, which can only be built valid.</summary>
public sealed record EmployeeBody(int Id, int CompanyId, int UserId, string FirstName, string LastName, List<RoleBody> Roles);
