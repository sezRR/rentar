using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Brands.Commands;

public sealed record DeleteBrandCommand(int Id);

public sealed class DeleteBrandCommandHandler(IBrandDal brandDal) : ICommandHandler<DeleteBrandCommand>
{
    public void Handle(DeleteBrandCommand command)
    {
        brandDal.Delete(new Brand { Id = command.Id });
    }
}
