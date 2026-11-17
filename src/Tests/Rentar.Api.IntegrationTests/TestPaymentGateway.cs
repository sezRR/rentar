using Rentar.Modules.Payments.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Api.IntegrationTests;

/// <summary>
/// Takes every payment unless a test makes it decline them, in place of the simulated gateway's coin toss.
/// </summary>
public sealed class TestPaymentGateway : IPaymentGateway
{
    private int _charges;

    public bool Declines { get; set; }

    /// <summary>How many payments were taken or declined since the last reset.</summary>
    public int Charges => _charges;

    public Task<bool> ChargeAsync(CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _charges);
        return Task.FromResult(!Declines);
    }

    public void Reset()
    {
        Declines = false;
        _charges = 0;
    }
}
