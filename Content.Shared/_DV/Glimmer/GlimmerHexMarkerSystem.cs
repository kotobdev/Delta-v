namespace Content.Shared._DV.Glimmer;

/// <summary>
/// This handles...
/// </summary>
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

        if (entity.Comp.Glimmer == 1000)
        {
            //ermmmm explode!!
        }
    }

    private void HexCollapse(Entity<GlimmerHexMarkerComponent> entity)
    {

    }

    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);


    }
}
