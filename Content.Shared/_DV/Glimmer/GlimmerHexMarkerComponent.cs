using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._DV.Glimmer;

/// <summary>
/// Marks a hex entity for the locational glimmer system.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlimmerHexMarkerComponent : Component // TODO: probably just rename GlimmerHexComponent
{
    [DataField]
    public string HexName = "IF YOU SEE THIS SOMETHING IS WRONG"; // godo

    [DataField, AutoNetworkedField]
    public int Glimmer = 0;

    [DataField]
    public int CorruptionStartThreshold = 700;
}
