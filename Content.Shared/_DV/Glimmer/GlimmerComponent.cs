using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Glimmer;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class GlimmerComponent : Component
{
    [DataField]
    public EntProtoId MarkerPrototype = "GlimmerHexMarker";
}
