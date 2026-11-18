using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>Every rental, cancelled ones included, or one page of them.</summary>
public sealed record GetRentalsQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetRentalsQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalsQuery, Page<Rental>>
{
    public Task<Page<Rental>> HandleAsync(GetRentalsQuery query, CancellationToken cancellationToken = default)
    {
        return rentalDal.GetPageAsync(query.Page, cancellationToken);
    }
}
