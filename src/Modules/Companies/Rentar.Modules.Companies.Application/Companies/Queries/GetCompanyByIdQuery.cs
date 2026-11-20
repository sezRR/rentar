using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Companies.Queries;

/// <summary>
/// The company, for any of its employees; throws <see cref="NotFoundException"/> when <see cref="ActorUserId"/> does
/// not work there, whether or not the company exists.
/// </summary>
public sealed record GetCompanyByIdQuery(int ActorUserId, int CompanyId);

public sealed class GetCompanyByIdQueryHandler(IEmployeeDal employeeDal) : IQueryHandler<GetCompanyByIdQuery, Company>
{
    public async Task<Company> HandleAsync(GetCompanyByIdQuery query, CancellationToken cancellationToken = default)
    {
        var employee = await employeeDal.RequireEmployeeAsync(query.CompanyId, query.ActorUserId, cancellationToken);
        return employee.Company;
    }
}
