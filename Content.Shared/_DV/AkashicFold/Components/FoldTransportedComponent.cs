using Robust.Shared.Audio;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._DV.AkashicFold;

/// <summary>
/// Primary component for player entities who have had their minds sent to the Fold
/// </summary>
[RegisterComponent, AutoGenerateComponentPause]
public sealed partial class FoldTransportedComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan ExitFoldTime = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan ExitFoldAudioTime = default!;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    [AutoPausedField]
    public TimeSpan EnterFoldTime = default!;

    [DataField]
    public SoundSpecifier EnterWarningSound = new SoundPathSpecifier("/Audio/_DV/Effects/clang2.ogg");

    [DataField]
    public EntityUid foldBody;
}
