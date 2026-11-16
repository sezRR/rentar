using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Brands.Queries;

/// <summary>Answers the brand, or <c>null</c> when there is none with that id.</summary>
public sealed record GetBrandByIdQuery(int Id);

public sealed class GetBrandByIdQueryHandler(IBrandDal brandDal) : IQueryHandler<GetBrandByIdQuery, Brand>
{
    public Task<Brand> HandleAsync(GetBrandByIdQuery query, CancellationToken cancellationToken = default)
    {
        return brandDal.GetAsync(b => b.Id == query.Id, cancellationToken);
    }
}
