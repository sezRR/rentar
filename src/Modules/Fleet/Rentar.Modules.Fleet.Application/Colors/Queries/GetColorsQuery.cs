using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Colors.Queries;

/// <summary>Every color, or one page of them.</summary>
public sealed record GetColorsQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetColorsQueryHandler(IColorDal colorDal) : IQueryHandler<GetColorsQuery, Page<Color>>
{
    public Task<Page<Color>> HandleAsync(GetColorsQuery query, CancellationToken cancellationToken = default)
    {
        return colorDal.GetPageAsync(query.Page, cancellationToken);
    }
}
