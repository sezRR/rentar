using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Companies.Queries;

/// <summary>The companies the Identity user <see cref="UserId"/> works at, or one page of them.</summary>
public sealed record GetCompaniesByUserQuery(int UserId, PageRequest Page = null) : IPagedQuery;

public sealed class GetCompaniesByUserQueryHandler(ICompanyDal companyDal) : IQueryHandler<GetCompaniesByUserQuery, Page<Company>>
{
    public Task<Page<Company>> HandleAsync(GetCompaniesByUserQuery query, CancellationToken cancellationToken = default)
    {
        return companyDal.GetPageAsync(c => c.Employees.Any(e => e.UserId == query.UserId), query.Page, cancellationToken);
    }
}
