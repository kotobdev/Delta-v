namespace Content.Shared._DV.Glimmer;

public sealed class GlimmerHexMarkerSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
    }

    public void AddHexGlimmer(Entity<GlimmerHexMarkerComponent> entity, int glimmer)
    {
        entity.Comp.Glimmer = Math.Clamp(entity.Comp.Glimmer + glimmer, 0, 1000);

        if(TryComp<GlimmerCorruptingComponent>(entity, out var corrupter))
        {
            corrupter.Enabled = entity.Comp.Glimmer >= entity.Comp.CorruptionStartThreshold;
        }

        if (entity.Comp.Glimmer == 1000)
        {
            //ermmmm explode!!
        }

        Dirty(entity);
    }

    private void HexCollapse(Entity<GlimmerHexMarkerComponent> entity)
    {

    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);


    }
}
