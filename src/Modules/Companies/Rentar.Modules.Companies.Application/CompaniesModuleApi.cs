using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application;

public class CompaniesModuleApi(ICompanyDal companyDal, IEmployeeDal employeeDal) : ICompaniesModuleApi
{
    public async Task<IReadOnlyList<CompanySummary>> GetCompaniesAsync(
        IReadOnlyCollection<int> companyIds,
        CancellationToken cancellationToken = default)
    {
        if (companyIds.Count == 0)
        {
            return [];
        }

        var ids = companyIds.ToArray();
        var companies = await companyDal.GetAllAsync(c => ids.Contains(c.Id), cancellationToken);
        return companies.ConvertAll(c => new CompanySummary(c.Id, c.Name));
    }

    public async Task<EmployeeSummary> GetEmployeeAsync(int companyId, int userId, CancellationToken cancellationToken = default)
    {
        var employee = await employeeDal.GetByUserAsync(companyId, userId, cancellationToken);
        return employee is null
            ? null
            : new EmployeeSummary(employee.Id, employee.CompanyId, employee.Company.Name, employee.UserId, employee.Permissions());
    }
}
