using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>Every rental of the Fleet car <see cref="CarId"/>, or one page of them.</summary>
public sealed record GetRentalsByCarQuery(int CarId, PageRequest Page = null) : IPagedQuery;

public sealed class GetRentalsByCarQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalsByCarQuery, Page<Rental>>
{
    public Task<Page<Rental>> HandleAsync(GetRentalsByCarQuery query, CancellationToken cancellationToken = default)
    {
        return rentalDal.GetPageAsync(r => r.CarId == query.CarId, query.Page, cancellationToken);
    }
}
