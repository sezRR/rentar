namespace Rentar.Modules.Companies.Domain;

/// <summary>What the domain says when it refuses something; meant for the caller, like the application's messages.</summary>
public static class DomainMessages
{
    public const string CompanyNameMissing = "Company name must be set";
    public const string CompanyOwnerRoleNotOfCompany = "The owner's role must be one of the company's roles";

    public const string EmployeeNeedsRole = "An employee needs at least one role";
    public const string EmployeeRoleOfOtherCompany = "An employee can only hold roles of their company";

    public const string RoleNameMissing = "Role name must be set";
}
