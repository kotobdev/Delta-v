using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._DV.Movement.Components;

/// <summary>
/// Allows entities with EyeCursorOffset to toggle the offset on and off via an action.
/// Similar to EyeClosingComponent but for cursor offset functionality.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class EyeCursorOffsetToggleComponent : Component
{
    /// <summary>
    /// The prototype to grant to enable cursor offset toggling action.
    /// </summary>
    [DataField("eyeCursorOffsetToggleAction", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string EyeCursorOffsetToggleAction = "ActionToggleEyeCursorOffset";

    /// <summary>
    /// The actual cursor offset toggling action entity itself.
    /// </summary>
    [DataField]
    public EntityUid? EyeCursorOffsetToggleActionEntity;

    /// <summary>
    /// Whether the cursor offset is currently enabled or disabled.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public bool EyeCursorOffsetEnabled = true;

    /// <summary>
    /// Sound to play when enabling cursor offset.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? EyeCursorOffsetEnableSound;

    /// <summary>
    /// Sound to play when disabling cursor offset.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField, AutoNetworkedField]
    public SoundSpecifier? EyeCursorOffsetDisableSound;
}