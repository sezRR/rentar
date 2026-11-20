using System.Threading;
using System.Threading.Tasks;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Application.Common.Models;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>
/// Rentals that are not cancelled, with their car, user and the customer's company as they were when each was booked,
/// and the daily price it was booked at; later changes in the other modules, or the car, customer or user being gone,
/// leave a rental's details as they were. The company name is <c>null</c> for a customer that rented with no company.
/// </summary>
public sealed record GetRentalDetailsQuery(PageRequest Page = null) : IPagedQuery;

public sealed class GetRentalDetailsQueryHandler(IRentalDal rentalDal)
    : IQueryHandler<GetRentalDetailsQuery, Page<RentalDetailResponse>>
{
    public async Task<Page<RentalDetailResponse>> HandleAsync(GetRentalDetailsQuery query, CancellationToken cancellationToken = default)
    {
        var rentals = await rentalDal.GetPageAsync(r => r.Status != RentalStatus.Cancelled, query.Page, cancellationToken);

        // A rental keeps its own copy of what the other modules said when it was booked, so none of them is asked.
        return rentals.Map(rental => new RentalDetailResponse
        {
            RentalId = rental.Id,
            CarId = rental.CarId,
            RentDate = rental.RentDate,
            ReturnDate = rental.ReturnDate,
            BrandName = rental.Car.BrandName,
            ColorName = rental.Car.ColorName,
            Description = rental.Car.Description,
            DailyPrice = rental.DailyPrice,
            ModelYear = rental.Car.ModelYear,
            CompanyName = rental.Customer.CompanyName,
            UserName = rental.Customer.FirstName
        });
    }
}
