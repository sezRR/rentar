using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Brands.Queries;

public sealed record GetBrandsQuery;

public sealed class GetBrandsQueryHandler(IBrandDal brandDal) : IQueryHandler<GetBrandsQuery, List<Brand>>
{
    public List<Brand> Handle(GetBrandsQuery query)
    {
        return brandDal.GetAll();
    }
}
