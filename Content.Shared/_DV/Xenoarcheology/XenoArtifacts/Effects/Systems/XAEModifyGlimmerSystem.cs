using Content.Shared._DV.Glimmer;
using Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Components;
using Content.Shared.Xenoarchaeology.Artifact;

namespace Content.Shared._DV.Xenoarcheology.XenoArtifacts.Effects.Systems;

public sealed class XAEModifyGlimmerSystem : EntitySystem
{
    [Dependency] private readonly SharedGridGlimmerSystem _glimmer = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XAEModifyGlimmerComponent, XenoArtifactNodeActivatedEvent>(OnActivated);
    }

    private void OnActivated(Entity<XAEModifyGlimmerComponent> arti, ref XenoArtifactNodeActivatedEvent args)
    {
        var range = arti.Comp.Range;
        var current = _glimmer.GetGlimmer(arti);
        if (range.Min > current || current > range.Max)
            return;

        _glimmer.AddGlimmer(arti, arti.Comp.Change);
    }
}
