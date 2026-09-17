using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Glimmer;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlimmerComponent : Component
{
    [DataField]
    public EntProtoId MarkerPrototype = "GlimmerHexMarker";

    /// <summary>
    /// Dictionary for navmap usage of glimmer hex markers OUGHHH MY GODDDDDDDD
    /// </summary>
    [AutoNetworkedField]
    public Dictionary<NetEntity, GlimmerHexMapMarker> HexMapMarkers = new();
}

[Serializable, NetSerializable, DataDefinition]
public partial struct GlimmerHexMapMarker
{
    [DataField]
    public Vector2 Position;

    [DataField]
    public int GlimmerLevel;
}
