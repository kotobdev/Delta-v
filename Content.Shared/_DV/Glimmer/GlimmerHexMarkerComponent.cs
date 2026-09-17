using Robust.Shared.GameStates;

namespace Content.Shared._DV.Glimmer;

/// <summary>
/// Marks a hex entity for the locational glimmer system.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GlimmerHexMarkerComponent : Component
{
    public string HexName = "IF YOU SEE THIS SOMETHING IS WRONG"; // godo

    [AutoNetworkedField]
    public int Glimmer = 0;
}
