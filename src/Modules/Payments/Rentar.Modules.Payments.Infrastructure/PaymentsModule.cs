using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Modules.Payments.Application;
using Rentar.Modules.Payments.Application.Commands;
using Rentar.Modules.Payments.Application.Common.Interfaces;
using Rentar.Modules.Payments.Contracts;
using Rentar.Modules.Payments.Infrastructure.Gateways;
using Rentar.Modules.Payments.Infrastructure.Persistence;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Modules;
using Rentar.Shared.Infrastructure.Persistence;
using Rentar.Shared.Presentation.Endpoints;

namespace Rentar.Modules.Payments.Infrastructure;

/// <summary>
/// Taking payments, including the payment of every rental booked. It stores no payment data; its schema holds only its
/// outbox and inbox.
/// </summary>
public sealed class PaymentsModule : IModule
{
    public string Name => "Payments";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<PaymentsDbContext>(PaymentsDbContext.Schema);
        services.AddModuleMessaging<PaymentsDbContext>(typeof(IPaymentsModuleApi).Assembly, typeof(PaymentsModuleApi).Assembly);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);

        // Registered with the framework rather than Autofac, so a host (the tests) can put another gateway in its place.
        services.AddSingleton<IPaymentGateway, SimulatedPaymentGateway>();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<PaymentsModuleApi>().As<IPaymentsModuleApi>().SingleInstance();

        builder.RegisterUseCases(typeof(TakePaymentCommand).Assembly);
    }
}
