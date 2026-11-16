using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Brands.Queries;

/// <summary>Answers the brand, or <c>null</c> when there is none with that id.</summary>
public sealed record GetBrandByIdQuery(int Id);

public sealed class GetBrandByIdQueryHandler(IBrandDal brandDal) : IQueryHandler<GetBrandByIdQuery, Brand>
{
    public Brand Handle(GetBrandByIdQuery query)
    {
        return brandDal.Get(b => b.Id == query.Id);
    }
}
