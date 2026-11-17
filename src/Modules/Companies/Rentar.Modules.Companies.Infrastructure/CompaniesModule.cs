using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rentar.Modules.Companies.Application;
using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Application.Companies.Queries;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Infrastructure.Persistence;
using Rentar.Shared.Infrastructure.Messaging;
using Rentar.Shared.Infrastructure.Modules;
using Rentar.Shared.Infrastructure.Persistence;
using Rentar.Shared.Presentation.Endpoints;

namespace Rentar.Modules.Companies.Infrastructure;

/// <summary>
/// Companies registered on Rentar, their employees (Identity users) and the roles that say what each employee may do
/// in their company.
/// </summary>
public sealed class CompaniesModule : IModule
{
    public string Name => "Companies";

    public void AddServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<CompaniesDbContext>(CompaniesDbContext.Schema);
        services.AddModuleMessaging<CompaniesDbContext>(typeof(ICompaniesModuleApi).Assembly, typeof(CompaniesModuleApi).Assembly);
        services.AddEndpoints(Presentation.AssemblyReference.Assembly);
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<EfCompanyDal>().As<ICompanyDal>().SingleInstance();
        builder.RegisterType<EfEmployeeDal>().As<IEmployeeDal>().SingleInstance();
        builder.RegisterType<EfRoleDal>().As<IRoleDal>().SingleInstance();
        builder.RegisterType<CompaniesModuleApi>().As<ICompaniesModuleApi>().SingleInstance();

        builder.RegisterUseCases(typeof(GetCompaniesByUserQuery).Assembly);
    }
}
