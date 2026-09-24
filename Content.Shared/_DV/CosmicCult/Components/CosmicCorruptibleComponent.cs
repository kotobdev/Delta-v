using Robust.Shared.Prototypes;

namespace Content.Shared._DV.CosmicCult.Components;

/// <summary>
/// Indicates that an entity will be converted to the given prototype when corrupted by the Cosmic Cult
/// </summary>
[RegisterComponent]
public sealed partial class CosmicCorruptibleComponent : Component
{
    [DataField]
    public EntProtoId ConvertTo;

    /// <summary>
    /// A string referencing a preset in a CosmicCorruptingComponent.
    /// Used to let different CosmicCorruptibleComponents convert an entity to different things.
    /// </summary>
    [DataField]
    public string ConvertToPreset = string.Empty;
}
