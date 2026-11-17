using Rentar.Shared.Kernel.Entities;

namespace Rentar.Modules.Payments.Domain;

/// <summary>
/// Card details of a payment. Never stored: the module's schema holds only its outbox and inbox.
/// </summary>
public class Payment : IEntity
{
    public string CardNumber { get; init; }
    public string HolderName { get; init; }
    public string MounthOfExp { get; init; }
    public string YearOfExp { get; init; }
    public string CVC { get; init; }
    public int Amount { get; init; }
}
