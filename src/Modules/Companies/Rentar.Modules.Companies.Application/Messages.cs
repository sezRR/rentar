namespace Rentar.Modules.Companies.Application;

public static class Messages
{
    public const string UserNotFound = "User not found";
    public const string UserNotActive = "User is not active";
    public const string UserAlreadyEmployee = "User is already an employee of the company";
    public const string RoleNotFound = "Role not found in the company";
    public const string CannotGrantMissingPermissions = "You can not grant a role with permissions you do not have";
    public const string CannotRemoveEmployeeWithMorePermissions = "You can not remove an employee with permissions you do not have";
    public const string CompanyNeedsFullAccessEmployee = "The company must keep an employee who holds every permission";

    /// <summary>Worded like the v2 not-found problems, so an outsider can not tell a company exists.</summary>
    public static string CompanyNotFound(int companyId) => $"Company {companyId} was not found.";

    public static string EmployeeNotFound(int employeeId) => $"Employee {employeeId} was not found.";

    public static string PermissionMissing(string permission) => $"Your roles in the company do not grant {permission}";
}
