using System.Linq;
using System.Numerics;
using Content.Server._DV.CosmicCult.EntitySystems;
using Content.Shared._DV.CosmicCult.Components;
using Content.Shared._DV.Glimmer;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._DV.Glimmer;

public sealed class GlimmerCorruptingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _rand = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDefinition = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TileSystem _tile = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly CosmicCorruptingSystem _cosmicCorrupting = default!;

    // This system does a lot of the same things as CosmicCorruptingSystem, except in a way that's different enough
    // that I felt inclined to make a separate system for it. Expect some overlap, though.
    public override void Update(float deltaTime)
    {
        base.Update(deltaTime);

        var query = EntityQueryEnumerator<GlimmerCorruptingComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.Enabled && _timing.CurTime >= comp.CorruptionTimer)
            {
                comp.CorruptionTimer = _timing.CurTime + comp.CorruptionSpeed;
                ConvertTile((uid, comp));
            }
        }
    }

    // This can iterate over the same tile multiple times, but it's not an issue worth fixing.
    private void ConvertTile(Entity<GlimmerCorruptingComponent> ent)
    {
        var xform = Transform(ent);

        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var mapGrid))
            return;

        var tilePos = xform.Coordinates.Offset(CalculateOffset(ent.Comp.Radius));

        if (_map.TryGetTileRef(gridUid, mapGrid, tilePos, out var tileRef) && tileRef.Tile.IsEmpty)
        {
            Log.Info("Tile ref at pos " + tilePos + "is empty");
            return;
        }

        Log.Info("replacing tile at pos " + tilePos + " with " + ent.Comp.ConversionTile);
        var convertTile = (ContentTileDefinition)_tileDefinition[ent.Comp.ConversionTile];
        _map.SetTile(gridUid, mapGrid, tilePos, new Tile(convertTile.TileId, variant: _tile.PickVariant(convertTile)));

        foreach (var convertedEnt in _map.GetAnchoredEntities((gridUid, mapGrid), tilePos).ToList())
        {
            var proto = Prototype(convertedEnt);
            if (ent.Comp.EntityConversionDict.TryGetValue(proto?.ID!, out var conversion))
            {
                _cosmicCorrupting.ConvertEntity(convertedEnt, conversion);
            }
            else if (TryComp<CosmicCorruptibleComponent>(convertedEnt, out var corruptible))
            {
                // We're ignoring ConvertTo presets, if they exist. There's like 19 files full of them, and we don't
                // need to add fold versions of ALL of them. Just walls, really.
                if (!ent.Comp.PresetEntityConversionDict.TryGetValue(corruptible.ConvertToPreset, out var value))
                    continue;

                _cosmicCorrupting.ConvertEntity(convertedEnt, value);
            }
        }
    }

    // don't ask me how this works
    private Vector2 CalculateOffset(float radius)
    {
        var t = _rand.NextFloat();
        t = MathF.Pow(t, 4f);

        var angle = _rand.NextFloat(0, MathF.Tau);
        var distance = t * radius;

        return new Vector2(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance);
    }
}
