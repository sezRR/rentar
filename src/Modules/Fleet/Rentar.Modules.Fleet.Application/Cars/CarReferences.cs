using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars;

/// <summary>The records a car points at, checked by every use case that stores one.</summary>
internal static class CarReferences
{
    /// <summary>
    /// Throws <see cref="BusinessRuleException"/> when the brand or the color is not stored. The foreign keys refuse
    /// such a car too; asking first turns that into an answer the caller can act on.
    /// </summary>
    public static async Task EnsureExistAsync(
        IBrandDal brandDal,
        IColorDal colorDal,
        int brandId,
        int colorId,
        CancellationToken cancellationToken)
    {
        if (!await brandDal.AnyAsync(b => b.Id == brandId, cancellationToken))
        {
            throw new BusinessRuleException(Messages.BrandNotFound);
        }

        if (!await colorDal.AnyAsync(c => c.Id == colorId, cancellationToken))
        {
            throw new BusinessRuleException(Messages.ColorNotFound);
        }
    }
}
