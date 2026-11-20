using Rentar.Modules.Payments.Application.Commands;
using Rentar.Modules.Payments.Contracts;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Payments.Application;

public class PaymentsModuleApi(ICommandHandler<TakePaymentCommand, bool> takePayment) : IPaymentsModuleApi
{
    public Task<bool> PayAsync(CancellationToken cancellationToken = default)
    {
        return takePayment.HandleAsync(new TakePaymentCommand(), cancellationToken);
    }
}
