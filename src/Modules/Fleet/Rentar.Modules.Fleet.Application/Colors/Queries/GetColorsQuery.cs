using System.Collections.Generic;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Colors.Queries;

public sealed record GetColorsQuery;

public sealed class GetColorsQueryHandler(IColorDal colorDal) : IQueryHandler<GetColorsQuery, List<Color>>
{
    public List<Color> Handle(GetColorsQuery query)
    {
        return colorDal.GetAll();
    }
}
