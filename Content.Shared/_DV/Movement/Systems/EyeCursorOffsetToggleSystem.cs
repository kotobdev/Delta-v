using Content.Shared.Actions;
using Content.Shared._DV.Movement.Components;
using Content.Shared.Movement.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._DV.Movement.Systems;

public sealed class EyeCursorOffsetToggleSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EyeCursorOffsetToggleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<EyeCursorOffsetToggleComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<EyeCursorOffsetToggleComponent, ToggleEyeCursorOffsetActionEvent>(OnToggleAction);
        SubscribeLocalEvent<EyeCursorOffsetToggleComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnMapInit(Entity<EyeCursorOffsetToggleComponent> toggle, ref MapInitEvent args)
    {
        _actionsSystem.AddAction(toggle, ref toggle.Comp.EyeCursorOffsetToggleActionEntity, toggle.Comp.EyeCursorOffsetToggleAction);
        Dirty(toggle);
    }

    private void OnShutdown(Entity<EyeCursorOffsetToggleComponent> toggle, ref ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(toggle, toggle.Comp.EyeCursorOffsetToggleActionEntity);
    }

    private void OnToggleAction(Entity<EyeCursorOffsetToggleComponent> toggle, ref ToggleEyeCursorOffsetActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        SetEyeCursorOffsetEnabled((toggle.Owner, toggle.Comp), !toggle.Comp.EyeCursorOffsetEnabled);
    }

    private void OnHandleState(Entity<EyeCursorOffsetToggleComponent> toggle, ref AfterAutoHandleStateEvent args)
    {
        DoAudioFeedback((toggle.Owner, toggle.Comp), toggle.Comp.EyeCursorOffsetEnabled);
    }

    /// <summary>
    /// Sets whether or not the entity's cursor offset is enabled.
    /// </summary>
    /// <param name="toggle">The entity that contains an EyeCursorOffsetToggleComponent</param>
    /// <param name="value">Set to true to enable cursor offset. Set to false to disable it</param>
    public void SetEyeCursorOffsetEnabled(Entity<EyeCursorOffsetToggleComponent?> toggle, bool value)
    {
        if (!Resolve(toggle, ref toggle.Comp))
            return;

        if (toggle.Comp.EyeCursorOffsetEnabled == value)
            return;

        toggle.Comp.EyeCursorOffsetEnabled = value;
        Dirty(toggle);

        if (toggle.Comp.EyeCursorOffsetToggleActionEntity != null)
            _actionsSystem.SetToggled(toggle.Comp.EyeCursorOffsetToggleActionEntity, !toggle.Comp.EyeCursorOffsetEnabled);

        // Note: We don't add/remove the EyeCursorOffsetComponent here, as that would interfere
        // with systems that expect it. Instead, the client system checks the toggle state.

        DoAudioFeedback(toggle, toggle.Comp.EyeCursorOffsetEnabled);
    }

    private void DoAudioFeedback(Entity<EyeCursorOffsetToggleComponent?> toggle, bool enabled)
    {
        if (!Resolve(toggle, ref toggle.Comp))
            return;

        if (_netManager.IsClient)
            return;

        var sound = enabled ? toggle.Comp.EyeCursorOffsetEnableSound : toggle.Comp.EyeCursorOffsetDisableSound;
        if (sound != null)
            _audio.PlayPvs(sound, toggle.Owner);
    }

    /// <summary>
    /// Gets whether the cursor offset is currently enabled.
    /// </summary>
    public bool IsEyeCursorOffsetEnabled(Entity<EyeCursorOffsetToggleComponent?> toggle)
    {
        return Resolve(toggle, ref toggle.Comp, false) && toggle.Comp.EyeCursorOffsetEnabled;
    }
}

public sealed partial class ToggleEyeCursorOffsetActionEvent : InstantActionEvent
{
}