using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Colors.Queries;

public sealed record GetColorsQuery;

public sealed class GetColorsQueryHandler(IColorDal colorDal) : IQueryHandler<GetColorsQuery, List<Color>>
{
    public Task<List<Color>> HandleAsync(GetColorsQuery query, CancellationToken cancellationToken = default)
    {
        return colorDal.GetAllAsync(cancellationToken);
    }
}
