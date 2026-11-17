using System;

namespace Rentar.Api.IntegrationTests;

/// <summary>A car image as the API sends it; the domain's <c>CarImage</c> can only be made by its car.</summary>
public sealed record CarImageBody(int Id, int CarId, string ImagePath, DateTime Date);
