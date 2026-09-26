using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._DV.Overlays;

public sealed class GlimmerOverlay : Overlay
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    public override OverlaySpace Space => OverlaySpace.WorldSpace;
    private readonly ShaderInstance _glimmerShader;
    private readonly ProtoId<ShaderPrototype> _shaderProto = "HighGlimmer";

    public float VisualGlimmerLevel = 0f;
    public int ActualGlimmerLevel = 0;

    public GlimmerOverlay()
    {
        IoCManager.InjectDependencies(this);
        _glimmerShader = _prototype.Index(_shaderProto).Instance().Duplicate();
    }

    protected override bool BeforeDraw(in OverlayDrawArgs args)
    {
        if (_player.LocalEntity == null)
        {
            return false;
        }

        return base.BeforeDraw(in args);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var lastFrameTime = (float) _timing.FrameTime.TotalSeconds;

        // lerp glimmer level to avoid jumps
        VisualGlimmerLevel = !MathHelper.CloseTo(VisualGlimmerLevel, ActualGlimmerLevel, 0.001f)
            ? float.Lerp(VisualGlimmerLevel, ActualGlimmerLevel, 0.6f * lastFrameTime) // increased from 0.1
            : ActualGlimmerLevel;

        // clamp glimmer to 0-1, map to exponential ease-out
        var progress = Math.Clamp((VisualGlimmerLevel - 500f) / 300f,0,1);
        var size = 1f - MathF.Pow(2f, -8f * progress);

        _glimmerShader.SetParameter("size",size);

        var worldHandle = args.WorldHandle;
        var viewport = args.WorldBounds;
        worldHandle.UseShader(_glimmerShader);
        worldHandle.DrawRect(viewport, Color.White);
        worldHandle.UseShader(null);
    }

    // used to avoid lerp jump if overlay was removed weirdly prior
    public void Reset()
    {
        VisualGlimmerLevel = 0f;
    }

}
