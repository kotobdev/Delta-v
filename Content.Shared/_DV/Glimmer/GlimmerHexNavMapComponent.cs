using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.Glimmer;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlimmerHexNavMapComponent : Component // IM GONNA DIEEEEE
{
    [DataField, AutoNetworkedField]
    public Dictionary<NetEntity, GlimmerHexBlip> Hexes = new();
}

[Serializable, NetSerializable, DataDefinition]
public partial struct GlimmerHexBlip
{
    [DataField]
    public Vector2 Position;

    [DataField]
    public string HexName;

    [DataField]
    public int Glimmer;
}
