using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Payments.Application;
using Rentar.Modules.Payments.Application.Commands;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;

namespace Rentar.Modules.Payments.Presentation.Endpoints.V1;

public class PaymentsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Payments", "payments", 1.0);

        group.MapGet("/pay", Pay);
    }

    private static Results<Ok<V1Response>, BadRequest<V1Response>> Pay(ICommandHandler<TakePaymentCommand, bool> takePayment)
    {
        return takePayment.Handle(new TakePaymentCommand())
            ? V1Results.Ok(Messages.PaymentSuccessful)
            : V1Results.Fail(Messages.PaymentFailed);
    }
}
