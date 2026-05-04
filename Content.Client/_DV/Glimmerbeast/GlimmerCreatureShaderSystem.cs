using Content.Shared._DV.Glimmerbeast;
using Content.Shared.Examine;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Prototypes;

namespace Content.Client._DV.Glimmerbeast;

/// TODO: rename this to something creature-agnostic
public sealed class GlimmerCreatureShaderSystem : EntitySystem
{
    private static readonly ProtoId<ShaderPrototype> Shader = "GlimmerCreature";

    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    private ShaderInstance _shader = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        _shader = _protoMan.Index(Shader).InstanceUnique();

        SubscribeLocalEvent<GlimmerCreatureShaderComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<GlimmerCreatureShaderComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GlimmerCreatureShaderComponent, ExaminedEvent>(OnExamined);
    }

    private void OnStartup(EntityUid uid, GlimmerCreatureShaderComponent component, ComponentStartup args)
    {
        SetShader(uid, true, component);
    }

    private void OnShutdown(EntityUid uid, GlimmerCreatureShaderComponent component, ComponentShutdown args)
    {
        if (!Terminating(uid))
            SetShader(uid, false, component);
    }

    private void OnExamined(EntityUid uid, GlimmerCreatureShaderComponent component, ExaminedEvent args)
    {
        if(component.ExaminedDesc != null)
            args.PushMarkup(Loc.GetString(component.ExaminedDesc, ("target", uid)));
    }

    private void SetShader(EntityUid uid, bool enabled, GlimmerCreatureShaderComponent? component = null, SpriteComponent? sprite = null)
    {
        if (!Resolve(uid, ref component, ref sprite, false))
            return;

        _sprite.SetColor((uid, sprite), Color.White);
        sprite.PostShader = enabled ? _shader : null;
        sprite.GetScreenTexture = enabled;
        sprite.RaiseShaderEvent = enabled;
    }
}
