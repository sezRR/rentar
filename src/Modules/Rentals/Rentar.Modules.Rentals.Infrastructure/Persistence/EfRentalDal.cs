using Microsoft.EntityFrameworkCore;
using Npgsql;
using Rentar.Modules.Rentals.Application;
using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Infrastructure.Persistence;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Infrastructure.Persistence;

public class EfRentalDal(IDbContextFactory<RentalsDbContext> contextFactory) : EfEntityRepositoryBase<Rental, RentalsDbContext>(contextFactory), IRentalDal
{
    public override async Task AddAsync(Rental entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await base.AddAsync(entity, cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.ExclusionViolation,
            ConstraintName: "EX_Rentals_CarPeriod" or "EX_Rentals_CustomerPeriod"
        } conflict)
        {
            // A competing booking can commit after the application precheck. It is the same business conflict,
            // not an unexpected database error, and no rental-created event has been published for this insert.
            throw new ConflictException(conflict.ConstraintName == "EX_Rentals_CarPeriod"
                ? Messages.CarNotAvailable
                : Messages.CustomerNotAvailable);
        }
    }
}
