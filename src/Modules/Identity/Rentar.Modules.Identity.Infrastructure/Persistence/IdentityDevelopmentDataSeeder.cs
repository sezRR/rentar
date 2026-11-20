using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Identity.Application.Auth.Commands;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Infrastructure.Persistence.Seeding;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Infrastructure.Persistence;

internal sealed class IdentityDevelopmentDataSeeder(
    IDbContextFactory<IdentityDbContext> contextFactory,
    ICommandHandler<RegisterUserCommand, User> registerUser) : IDevelopmentDataSeeder
{
    public int Order => 10;

    public async Task SeedAsync(DevelopmentSeedContext context, CancellationToken cancellationToken = default)
    {
        await using var database = await contextFactory.CreateDbContextAsync(cancellationToken);
        foreach (var (key, firstName, email) in new[]
        {
            ("identity.owner", "Owner", "owner@rentar.local"),
            ("identity.member", "Member", "member@rentar.local"),
            ("identity.renter", "Renter", "renter@rentar.local")
        })
        {
            var user = await database.Users.SingleOrDefaultAsync(user => user.Email == email, cancellationToken)
                ?? await registerUser.HandleAsync(new RegisterUserCommand(firstName, "Demo", email, "Passw0rd!"), cancellationToken);
            context.SetId(key, user.Id);
        }
    }
}
