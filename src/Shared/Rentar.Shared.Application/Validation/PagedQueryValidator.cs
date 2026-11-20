using FluentValidation;
using JetBrains.Annotations;
using Rentar.Shared.Application.Data;
using Rentar.Shared.Kernel.Paging;

namespace Rentar.Shared.Application.Validation;

/// <summary>
/// Checks the page every <see cref="IPagedQuery"/> asks for, next to the query's own validator if it has one.
/// Failures are named after the query string parameter the size is read from, <c>limit</c>.
/// </summary>
/// <remarks>Registered once for every module by <c>RegisterUseCaseValidation</c>.</remarks>
[UsedImplicitly]
public sealed class PagedQueryValidator<TQuery> : UseCaseValidator<TQuery> where TQuery : IPagedQuery
{
    public PagedQueryValidator()
    {
        When(q => q.Page is not null, () =>
        {
            RuleFor(q => q.Page.Size)
                .InclusiveBetween(1, PageRequest.MaxSize)
                .OverridePropertyName("limit")
                .WithMessage($"'limit' must be between 1 and {PageRequest.MaxSize}.");
        });
    }
}
