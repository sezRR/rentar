using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application;

public class CompaniesModuleApi(IEmployeeDal employeeDal) : ICompaniesModuleApi
{
    public async Task<EmployeeSummary> GetEmployeeAsync(int companyId, int userId, CancellationToken cancellationToken = default)
    {
        var employee = await employeeDal.GetByUserAsync(companyId, userId, cancellationToken);
        return employee is null
            ? null
            : new EmployeeSummary(employee.Id, employee.CompanyId, employee.Company.Name, employee.UserId, employee.Permissions());
    }
}
