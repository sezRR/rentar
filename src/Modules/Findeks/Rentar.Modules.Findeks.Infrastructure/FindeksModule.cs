using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Modules.Findeks.Application;
using Rentar.Modules.Findeks.Application.Common.Interfaces;
using Rentar.Modules.Findeks.Application.Queries;
using Rentar.Modules.Findeks.Contracts;
using Rentar.Modules.Findeks.Infrastructure.Persistence;
using Rentar.Shared.Infrastructure.Modules;
using Rentar.Shared.Infrastructure.Persistence;
using Rentar.Shared.Presentation.Endpoints;

namespace Rentar.Modules.Findeks.Infrastructure;

/// <summary>
/// Findeks credit ratings, and the scores customers were given. Customers are referenced by id only.
/// </summary>
public sealed class FindeksModule : IModule
{
    public string Name => "Findeks";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<FindeksDbContext>(FindeksDbContext.Schema);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<EfCustomerScoreDal>().As<ICustomerScoreDal>().SingleInstance();
        builder.RegisterType<FindeksModuleApi>().As<IFindeksModuleApi>().SingleInstance();

        builder.RegisterUseCases(typeof(CalculateFindeksRatingQuery).Assembly);
    }
}
