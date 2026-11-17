using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Identity.Application.Common.Interfaces;
using Rentar.Modules.Identity.Contracts.Events;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Application.Users.Commands;

/// <summary>
/// Makes the user inactive, so they can no longer sign in, and publishes <see cref="UserDeactivatedEvent"/>. Right
/// after, Companies removes them from every company, which ends their customer account's company, and Rentals cancels
/// their rentals that have not started. An inactive user stays as they are and nothing is published.
/// </summary>
/// <remarks>
/// Throws <see cref="NotFoundException"/> for an unknown user, and <see cref="ConflictException"/> while the user is
/// the last employee holding every permission at a company, which would leave nobody able to run it.
/// </remarks>
public sealed record DeactivateUserCommand(int Id);

public sealed class DeactivateUserCommandHandler(IUserDal userDal, ICompaniesModuleApi companiesModule, IEventPublisher eventPublisher)
    : ICommandHandler<DeactivateUserCommand>
{
    [TransactionScopeAspect]
    public async Task HandleAsync(DeactivateUserCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userDal.GetAsync(u => u.Id == command.Id, cancellationToken)
            ?? throw new NotFoundException(Messages.UserNotFound);

        if (!user.Status)
        {
            return;
        }

        // Companies' reaction refuses to take away a company's last full-access employee too, but only after this has
        // committed, where nobody hears of it; asking first turns it into an answer the caller can act on.
        if (await companiesModule.IsLastFullAccessEmployeeAsync(user.Id, cancellationToken))
        {
            throw new ConflictException(Messages.UserIsLastFullAccessEmployee);
        }

        await userDal.UpdateAsync(user.Deactivated(), cancellationToken);

        await eventPublisher.PublishAsync(new UserDeactivatedEvent(user.Id), cancellationToken);
    }
}
