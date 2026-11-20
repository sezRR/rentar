using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Commands;

/// <summary>
/// The payment for the rental <see cref="RentalId"/> was declined, so a pending one becomes
/// <see cref="RentalStatus.Cancelled"/> and frees its car. A rental that is not pending, or not stored, stays as it is.
/// </summary>
public sealed record CancelRentalCommand(int RentalId);

public sealed class CancelRentalCommandHandler(IRentalDal rentalDal) : ICommandHandler<CancelRentalCommand>
{
    public async Task HandleAsync(CancelRentalCommand command, CancellationToken cancellationToken = default)
    {
        var rental = await rentalDal.GetAsync(r => r.Id == command.RentalId, cancellationToken);
        if (rental is not { Status: RentalStatus.Pending })
        {
            return;
        }

        await rentalDal.UpdateAsync(rental.Cancelled(), cancellationToken);
    }
}
