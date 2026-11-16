using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Contracts;

/// <summary>
/// What other modules may ask the Companies module.
/// </summary>
public interface ICompaniesModuleApi
{
    /// <summary>Companies with the given ids; unknown ids are left out.</summary>
    Task<IReadOnlyList<CompanySummary>> GetCompaniesAsync(IReadOnlyCollection<int> companyIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// The Identity user's employment in the company with what it may do there, or <c>null</c> when the user is not
    /// one of its employees, which includes a company that does not exist.
    /// </summary>
    Task<EmployeeSummary> GetEmployeeAsync(int companyId, int userId, CancellationToken cancellationToken = default);
}
