using Content.Shared._DV.CCVars;
using Content.Shared._DV.Glimmer;
using Content.Shared.Psionics.Glimmer;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client._DV.Overlays;

public sealed partial class GlimmerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedGridGlimmerSystem _glimmer = default!;

    private GlimmerOverlay _overlay = default!;

    private bool _cvarDisabled;

    private TimeSpan? _nextUpdate;
    private static readonly TimeSpan Delay = TimeSpan.FromSeconds(0.25);

    private TimeSpan? _disableAt;
    private static readonly TimeSpan DisableDelay = TimeSpan.FromSeconds(5f);

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new GlimmerOverlay();
        _cfg.OnValueChanged(DCCVars.DisableGlimmerShader, OnDisableGlimmerShaderChanged);
        OnDisableGlimmerShaderChanged(_cfg.GetCVar(DCCVars.DisableGlimmerShader));
    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        if (_nextUpdate != null && _timing.CurTime < _nextUpdate)
            return;

        if (_player.LocalEntity is { } playerEnt)
        {
            var glimmer = _glimmer.GetGlimmer(playerEnt);
            _overlay.ActualGlimmerLevel = glimmer;
            if(glimmer > 500)
            {
                _disableAt = null;
                if (!_overlayMan.HasOverlay<GlimmerOverlay>())
                {
                    Log.Info("Turning on glimmer overlay! Reported value " + glimmer);
                    _overlay.Reset();
                    _overlayMan.AddOverlay(_overlay);
                }
            }
            else
            {
                _disableAt ??= _timing.CurTime + DisableDelay;
                if (_overlayMan.HasOverlay<GlimmerOverlay>() && _timing.CurTime >= _disableAt)
                {
                    Log.Info("Turning off glimmer overlay! Reported value " + glimmer);
                    _overlayMan.RemoveOverlay(_overlay);
                    _disableAt = null;
                }
            }
            //Log.Info("We have player ent and glimmer is " + glimmer);
        }

        _nextUpdate = _timing.CurTime + Delay;
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _overlayMan.RemoveOverlay<GlimmerOverlay>();
    }

    private void OnDisableGlimmerShaderChanged(bool enabled)
    {
        _cvarDisabled = enabled;
        if (enabled)
            _overlayMan.RemoveOverlay(_overlay);
        else if (_overlay.ActualGlimmerLevel > 700)
            _overlayMan.AddOverlay(_overlay);
    }

}
