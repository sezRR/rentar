using Rentar.Modules.Payments.Application.Commands;
using Rentar.Modules.Payments.Contracts;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Payments.Application;

public class PaymentsModuleApi(ICommandHandler<TakePaymentCommand, bool> takePayment) : IPaymentsModuleApi
{
    public bool Pay()
    {
        return takePayment.Handle(new TakePaymentCommand());
    }
}
