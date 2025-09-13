using System.Numerics;
using Content.Client.Movement.Components;
using Content.Client.Movement.Systems;
using Content.Shared._DV.Movement.Components;
using Content.Shared.Camera;
using Content.Shared.Movement.Components;

namespace Content.Client._DV.Movement.Systems;

/// <summary>
/// Client-side system that extends the EyeCursorOffsetSystem to respect the toggle state.
/// </summary>
public sealed class EyeCursorOffsetToggleSystem : EntitySystem
{
    [Dependency] private readonly EyeCursorOffsetSystem _eyeCursorOffsetSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        // Subscribe with higher priority to override the base system's behavior
        SubscribeLocalEvent<EyeCursorOffsetComponent, GetEyeOffsetEvent>(OnGetEyeOffsetEvent, before: [typeof(EyeCursorOffsetSystem)]);
    }

    private void OnGetEyeOffsetEvent(EntityUid uid, EyeCursorOffsetComponent component, ref GetEyeOffsetEvent args)
    {
        // Check if the entity has a toggle component and if the offset is disabled
        if (TryComp<EyeCursorOffsetToggleComponent>(uid, out var toggle) && !toggle.EyeCursorOffsetEnabled)
        {
            // If disabled, don't add any offset
            return;
        }

        // If enabled or no toggle component exists, use the original behavior
        var offset = _eyeCursorOffsetSystem.OffsetAfterMouse(uid, component);
        if (offset == null)
            return;

        args.Offset += offset.Value;
    }
}