using Content.Shared._DV.CCVars;
using Content.Shared.Psionics.Glimmer;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Client._DV.Overlays;

public sealed partial class GlimmerOverlaySystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlayMan = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private GlimmerOverlay _overlay = default!;

    private bool _cvarDisabled;
    private TimeSpan? _foldEffectStartTime;
    private TimeSpan? _foldEffectEndTime;

    public override void Initialize()
    {
        base.Initialize();

        _overlay = new GlimmerOverlay();
        SubscribeNetworkEvent<GlimmerChangedEvent>(OnGlimmerChanged);
        _cfg.OnValueChanged(DCCVars.DisableGlimmerShader, OnDisableGlimmerShaderChanged);
        OnDisableGlimmerShaderChanged(_cfg.GetCVar(DCCVars.DisableGlimmerShader));
    }

    private void OnGlimmerChanged(GlimmerChangedEvent eventArgs)
    {
        if(_cvarDisabled)
            return;

        // don't want this messing with an intentional fold effect
        if (_foldEffectEndTime != null)
            return;

        if(eventArgs.Glimmer > 700)
        {
            _overlay.ActualGlimmerLevel = eventArgs.Glimmer;
            if (!_overlayMan.HasOverlay<GlimmerOverlay>())
            {
                _overlay.Reset();
                _overlayMan.AddOverlay(_overlay);
            }
        }
        else
        {
            if (_overlayMan.HasOverlay<GlimmerOverlay>())
            {
                _overlayMan.RemoveOverlay(_overlay);
            }
        }
    }

    // this represents a pretty specific effect, shouldn't make this a general function
    public void FoldTransportEnable(TimeSpan startTime, TimeSpan endTime)
    {
        if (_cvarDisabled)
            return;

        _foldEffectStartTime = startTime;
        _foldEffectEndTime = endTime;

        if (!_overlayMan.HasOverlay<GlimmerOverlay>())
        {
            _overlay.Reset();
            _overlayMan.AddOverlay(_overlay);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_foldEffectEndTime == null || _foldEffectStartTime == null)
            return;

        if (_timing.CurTime > _foldEffectEndTime)
        {
            _overlayMan.RemoveOverlay(_overlay);
            _foldEffectEndTime = null;
            _foldEffectStartTime = null;
            return;
        }

        // Here, we map how far we are to the end to a value of 0-1, and translate that to
        // glimmer value levels the overlay expects.
        var t = (_timing.CurTime - _foldEffectStartTime) / (_foldEffectEndTime - _foldEffectStartTime);
        t = (t * 300) + 700;

        Log.Info("GLIMMER THINGY UPDATE! " + t);

        _overlay.ActualGlimmerLevel = (int)t;
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
