using Content.Shared._DV.Glimmer;
using Robust.Shared.Timing;

namespace Content.Server._DV;

/// <inheritdoc/>
public sealed class GlimmerHexMapSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    // Please don't kill me.
    private static TimeSpan UpdateInterval = TimeSpan.FromSeconds(1f);
    private TimeSpan? _nextUpdate;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlimmerHexMarkerComponent, MapInitEvent>(OnHexInit);
        SubscribeLocalEvent<GlimmerHexMarkerComponent, EntityTerminatingEvent>(OnHexRemoved);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextUpdate != null && _timing.CurTime < _nextUpdate)
            return;

        _nextUpdate = _timing.CurTime + UpdateInterval;

        var query = EntityQueryEnumerator<GlimmerHexMarkerComponent>();
        while (query.MoveNext(out var uid, out var marker))
        {
            RefreshHex((uid, marker));
        }
    }

    private void OnHexInit(Entity<GlimmerHexMarkerComponent> entity, ref MapInitEvent args)
    {
        RefreshHex(entity);
    }

    private void RefreshHex(Entity<GlimmerHexMarkerComponent> entity)
    {
        var xform = Transform(entity);

        if (xform.GridUid is not { } gridUid)
            return;

        var mapComp = EnsureComp<GlimmerHexNavMapComponent>(gridUid);
        var netEntity = GetNetEntity(entity);
        var localPos = xform.LocalPosition;

        if (mapComp.Hexes.TryGetValue(netEntity, out var existing) && existing.Position == localPos &&
            existing.Glimmer == entity.Comp.Glimmer && existing.HexName == entity.Comp.HexName)
            return;

        mapComp.Hexes[netEntity] = new GlimmerHexBlip
        {
            Position = localPos,
            HexName = entity.Comp.HexName,
            Glimmer = entity.Comp.Glimmer
        };

        Dirty(gridUid, mapComp);
    }

    private void OnHexRemoved(Entity<GlimmerHexMarkerComponent> entity, ref EntityTerminatingEvent args)
    {
        var xform = Transform(entity);

        if (xform.GridUid is not { } gridUid || !TryComp<GlimmerHexNavMapComponent>(gridUid, out var mapComp))
            return;

        if (mapComp.Hexes.Remove(GetNetEntity(entity)))
            Dirty(gridUid, mapComp);
    }
}
