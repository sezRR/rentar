using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Commands;

/// <summary>
/// The rental <see cref="RentalId"/> was paid for, so a pending one becomes <see cref="RentalStatus.Confirmed"/>. A
/// rental that is not pending, or not stored, stays as it is.
/// </summary>
public sealed record ConfirmRentalCommand(int RentalId);

public sealed class ConfirmRentalCommandHandler(IRentalDal rentalDal) : ICommandHandler<ConfirmRentalCommand>
{
    public async Task HandleAsync(ConfirmRentalCommand command, CancellationToken cancellationToken = default)
    {
        var rental = await rentalDal.GetAsync(r => r.Id == command.RentalId, cancellationToken);
        if (rental is not { Status: RentalStatus.Pending })
        {
            return;
        }

        await rentalDal.UpdateAsync(rental.Confirmed(), cancellationToken);
    }
}
