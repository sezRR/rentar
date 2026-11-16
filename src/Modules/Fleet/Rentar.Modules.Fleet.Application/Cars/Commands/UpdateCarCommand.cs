using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;

namespace Rentar.Modules.Fleet.Application.Cars.Commands;

public sealed record UpdateCarCommand(
    int Id,
    int BrandId,
    int ColorId,
    int ModelYear,
    decimal DailyPrice,
    string Description,
    int MinimumFindeksRating);

public sealed class UpdateCarCommandValidator : UseCaseValidator<UpdateCarCommand>
{
    public UpdateCarCommandValidator()
    {
        RuleFor(c => c.Description).ValidDescription();
        RuleFor(c => c.ModelYear).ValidModelYear();
        RuleFor(c => c.DailyPrice).ValidDailyPrice();
    }
}

public sealed class UpdateCarCommandHandler(ICarDal carDal) : ICommandHandler<UpdateCarCommand>
{
    public void Handle(UpdateCarCommand command)
    {
        carDal.Update(new Car
        {
            Id = command.Id,
            BrandId = command.BrandId,
            ColorId = command.ColorId,
            ModelYear = command.ModelYear,
            DailyPrice = command.DailyPrice,
            Description = command.Description,
            MinimumFindeksRating = command.MinimumFindeksRating
        });
    }
}
