using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Brands.Queries;

public sealed record GetBrandsQuery;

public sealed class GetBrandsQueryHandler(IBrandDal brandDal) : IQueryHandler<GetBrandsQuery, List<Brand>>
{
    public Task<List<Brand>> HandleAsync(GetBrandsQuery query, CancellationToken cancellationToken = default)
    {
        return brandDal.GetAllAsync(cancellationToken);
    }
}
