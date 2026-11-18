using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Kernel.Paging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Common.Interfaces;

public interface IEmployeeDal : IEntityRepository<Employee>
{
    /// <summary>The company's employees in the order they joined, each with its roles.</summary>
    Task<List<Employee>> GetByCompanyAsync(int companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// One page of the company's employees in the order they joined, each with its roles; a <c>null</c> page answers
    /// all of them.
    /// </summary>
    Task<Page<Employee>> GetPageByCompanyAsync(int companyId, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// The user's employee record in the company with its roles and company, or <c>null</c> when the user does not
    /// work there.
    /// </summary>
    Task<Employee> GetByUserAsync(int companyId, int userId, CancellationToken cancellationToken = default);

    /// <summary>Stores a new employee and grants it its <see cref="Employee.Roles"/>, which must be stored already.</summary>
    Task AddWithRolesAsync(Employee employee, CancellationToken cancellationToken = default);
}
