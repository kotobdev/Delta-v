using Content.Shared.Maps;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._DV.Glimmer;

[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class GlimmerCorruptingComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField] public TimeSpan CorruptionTimer;

    [DataField]
    public bool Enabled = true;

    [DataField]
    public float Radius = 15f; //should match glimmer hex radius. roughly

    [DataField]
    public ProtoId<ContentTileDefinition> ConversionTile = "FloorChromite";

    [DataField]
    public Dictionary<EntProtoId, EntProtoId> EntityConversionDict;

    /// <summary>
    /// Dictionary for what entities with CosmicCorruptibleComponent ConvertToPreset strings should be converted to.
    /// </summary>
    [DataField]
    public Dictionary<string, EntProtoId> PresetEntityConversionDict;

    /// <summary>
    /// How much time between tile corruptions.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan CorruptionSpeed = TimeSpan.FromSeconds(6);
}
