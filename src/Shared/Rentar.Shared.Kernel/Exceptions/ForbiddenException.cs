namespace Rentar.Shared.Kernel.Exceptions;

/// <summary>
/// The caller is known but may not do this, e.g. an employee whose roles lack the permission the operation needs.
/// </summary>
public class ForbiddenException(string message) : RentarException(message);
