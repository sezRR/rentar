using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Brands.Queries;

/// <summary>Every brand, or one page of them.</summary>
public sealed record GetBrandsQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetBrandsQueryHandler(IBrandDal brandDal) : IQueryHandler<GetBrandsQuery, Page<Brand>>
{
    public Task<Page<Brand>> HandleAsync(GetBrandsQuery query, CancellationToken cancellationToken = default)
    {
        return brandDal.GetPageAsync(query.Page, cancellationToken);
    }
}
