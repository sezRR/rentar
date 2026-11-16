namespace Rentar.Modules.Companies.Presentation.Endpoints.V2;

/// <summary>Body for registering or renaming a company; who is asking comes from the access token.</summary>
public sealed record CompanyRequest(string Name);
