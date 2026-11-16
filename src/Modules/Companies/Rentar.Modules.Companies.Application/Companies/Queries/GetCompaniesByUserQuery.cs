using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Companies.Queries;

/// <summary>The companies the Identity user <see cref="UserId"/> works at.</summary>
public sealed record GetCompaniesByUserQuery(int UserId);

public sealed class GetCompaniesByUserQueryHandler(ICompanyDal companyDal) : IQueryHandler<GetCompaniesByUserQuery, List<Company>>
{
    public async Task<List<Company>> HandleAsync(GetCompaniesByUserQuery query, CancellationToken cancellationToken = default)
    {
        var companies = await companyDal.GetAllAsync(c => c.Employees.Any(e => e.UserId == query.UserId), cancellationToken);
        return companies.OrderBy(c => c.Id).ToList();
    }
}
