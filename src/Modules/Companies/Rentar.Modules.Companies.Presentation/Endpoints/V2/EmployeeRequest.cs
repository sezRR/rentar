using System.Collections.Generic;

namespace Rentar.Modules.Companies.Presentation.Endpoints.V2;

/// <summary>Body for employing an Identity user with some of the company's roles.</summary>
public sealed record EmployeeRequest(int UserId, List<int> RoleIds);
