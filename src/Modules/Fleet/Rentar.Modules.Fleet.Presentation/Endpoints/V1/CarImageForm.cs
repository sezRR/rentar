namespace Rentar.Modules.Fleet.Presentation.Endpoints.V1;

/// <summary>
/// The image fields v1 sends with <c>carimages/update</c>. Only the id is read: the image stays with its car.
/// </summary>
public sealed class CarImageForm
{
    public int Id { get; init; }
}
