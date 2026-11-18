using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Application.Companies.Commands;
using Rentar.Modules.Companies.Application.Employees.Commands;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Infrastructure.Persistence;

internal sealed class CompaniesDevelopmentDataSeeder(
    IDbContextFactory<CompaniesDbContext> contextFactory,
    ICommandHandler<RegisterCompanyCommand, Company> registerCompany,
    ICommandHandler<AddEmployeeCommand, EmployeeResponse> addEmployee) : IDevelopmentDataSeeder
{
    public int Order => 30;

    public async Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        var ownerId = context.GetId("identity.owner");
        var memberId = context.GetId("identity.member");
        var company = await database.Companies.FirstOrDefaultAsync(
            company => company.Name == "Rentar Demo" && company.Employees.Any(employee => employee.UserId == ownerId),
            cancellationToken)
            ?? await registerCompany.HandleAsync(new RegisterCompanyCommand(ownerId, "Rentar Demo"), cancellationToken);

        if (!await database.Employees.AnyAsync(employee => employee.CompanyId == company.Id && employee.UserId == memberId, cancellationToken))
        {
            var memberRole = await database.Roles.SingleAsync(role => role.CompanyId == company.Id && role.Name == "Member", cancellationToken);
            await addEmployee.HandleAsync(new AddEmployeeCommand(ownerId, company.Id, memberId, [memberRole.Id]), cancellationToken);
        }

        context.SetId("companies.demo", company.Id);
    }
}
