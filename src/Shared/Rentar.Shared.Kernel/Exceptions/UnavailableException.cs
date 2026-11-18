namespace Rentar.Shared.Kernel.Exceptions;

/// <summary>
/// The operation is temporarily unavailable.
/// </summary>
public class UnavailableException(string message) : RentarException(message);
