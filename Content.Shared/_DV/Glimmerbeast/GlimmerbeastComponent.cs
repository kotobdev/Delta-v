using Robust.Shared.Prototypes;

namespace Content.Shared._DV.Glimmerbeast;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class GlimmerbeastComponent : Component
{
    [DataField]
    public string ExaminedDesc = "glimmerbeast-examined-desc";

    [DataField]
    public string NameOverride = "glimmerbeast-name-override";

    [DataField]
    public string DeathPopupText = "glimmerbeast-death-popup";

    [DataField]
    public EntProtoId? DespawnVfx;
}
