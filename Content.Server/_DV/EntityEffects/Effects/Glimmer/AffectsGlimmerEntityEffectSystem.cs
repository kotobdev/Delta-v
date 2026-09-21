using Content.Shared.EntityEffects;
using Content.Shared._DV.EntityEffects.Effects.Glimmer;
using Content.Shared._DV.Glimmer;

namespace Content.Server._DV.EntityEffects.Effects.Glimmer;

/// <summary>
///     Changes glimmer when reaction happens.
/// </summary>
/// <inheritdoc cref="EntityEffectSystem{T, TEffect}"/>
public sealed partial class AffectsGlimmerEntityEffectSystem : EntityEffectSystem<MetaDataComponent, AffectsGlimmer>
{
    [Dependency] private readonly SharedGridGlimmerSystem _glimmer = default!;
    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<AffectsGlimmer> args)
    {
        _glimmer.AddGlimmer(entity, args.Effect.Amount);
    }
}
