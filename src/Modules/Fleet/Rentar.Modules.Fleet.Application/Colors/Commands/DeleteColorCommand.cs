using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Colors.Commands;

public sealed record DeleteColorCommand(int Id);

public sealed class DeleteColorCommandHandler(IColorDal colorDal) : ICommandHandler<DeleteColorCommand>
{
    public void Handle(DeleteColorCommand command)
    {
        colorDal.Delete(new Color { Id = command.Id });
    }
}
