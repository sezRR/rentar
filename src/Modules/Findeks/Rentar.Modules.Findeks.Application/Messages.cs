namespace Rentar.Modules.Findeks.Application;

public static class Messages
{
    public const string FindeksRatingCalculated = "Findeks rating calculated successfully";

    /// <summary>Worded like the v2 not-found problems.</summary>
    public static string CustomerNotFound(int customerId) => $"Customer {customerId} was not found.";
}
