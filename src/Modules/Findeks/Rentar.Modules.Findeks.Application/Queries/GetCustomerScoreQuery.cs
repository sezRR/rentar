using Rentar.Modules.Findeks.Application.Common.Interfaces;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Findeks.Application.Queries;

/// <summary>Answers the customer's scores, or <c>null</c> when none were calculated for that customer.</summary>
public sealed record GetCustomerScoreQuery(int CustomerId);

public sealed class GetCustomerScoreQueryHandler(ICustomerScoreDal customerScoreDal) : IQueryHandler<GetCustomerScoreQuery, CustomerScore>
{
    public Task<CustomerScore> HandleAsync(GetCustomerScoreQuery query, CancellationToken cancellationToken = default)
    {
        return customerScoreDal.GetAsync(s => s.CustomerId == query.CustomerId, cancellationToken);
    }
}
