using System.Numerics;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._DV.Glimmer;

// TODO: rename all of these once you rip the old systems out

/// <summary>
/// This handles...
/// </summary>
public sealed class GridGlimmerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;

    private EntityQuery<GlimmerHexMarkerComponent> _hexQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlimmerComponent, ComponentInit>(OnInit); //it's MY system so I get to decide the setup time!

        _hexQuery = GetEntityQuery<GlimmerHexMarkerComponent>();
    }

    public void AddGlimmer(EntityUid? ent, float amount, float radius = 20)
    {
        if (ent == null)
            return;

        var sourcePos = _transform.GetWorldPosition(ent.Value);

        var targets = new List<(Entity<GlimmerHexMarkerComponent> marker, float distance)>();
        Entity<GlimmerHexMarkerComponent>? closestMarker = null;
        var closestDistance = float.MaxValue;

        Log.Info("AddGlimmer iterating over targets now...");

        var query = EntityQueryEnumerator<GlimmerHexMarkerComponent>();
        while (query.MoveNext(out var marker, out var comp))
        {
            var distance = Vector2.Distance(sourcePos, _transform.GetWorldPosition(marker));

            Log.Info($"iterating over marker {comp.HexName} at distance {distance}");

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestMarker = (marker, comp);
            }

            if (distance <= radius)
                targets.Add(((marker, comp), distance));
        }

        // nothing in radius, just dump the glimmer on nearest marker
        if (targets.Count == 0 && closestMarker != null)
        {
            targets.Add((closestMarker.Value, closestDistance));
        }

        // i barely understnad how this shit works. inverse-distance weighting google it or something
        var totalWeight = 0f;

        if(targets.Count == 0)
            Log.Info("well uhm. uh. no targets...");

        foreach (var (marker, distance) in targets)
        {
            var weight = 1f / (distance + 0.01f); //0.01 avoid divide by 0
            totalWeight += weight;
        }

        foreach (var (marker, distance) in targets)
        {
            var weight = (1f / MathF.Max(distance, 0.01f)) / totalWeight;
            marker.Comp.Glimmer += (int)(amount * weight); // i love casting to ints!! i love casting to ints!!!
            Log.Info($"Added {amount * weight} glimmer to {marker.Comp.HexName} (total: {marker.Comp.Glimmer}). Distance was {distance}, weight was {weight}");
        }
    }

    private void OnInit(Entity<GlimmerComponent> ent, ref ComponentInit args)
    {
        //oh boy i can't way to shitcode my way into this

        Log.Info("init'd");

        if (!TryComp<StationDataComponent>(ent, out var stationData))
            return;

        if (_station.GetLargestGrid((ent, stationData)) is not { } gridEnt) // i dont even know
            return;

        if (!TryComp<MapGridComponent>(gridEnt, out var mapGrid))
            return;

        Log.Info("made it past mapgrid check");

        var spacing = 20f; // todo: move this to comp
        var rowSpacing = spacing * MathF.Sqrt(3f) / 2f;

        var bounds = mapGrid.LocalAABB;

        var yIter = 0;
        var xIter = 0;
        for (var y = bounds.Bottom; y <= bounds.Top; y += rowSpacing)
        {
            yIter++;
            Log.Info($"Y iteration {y}");

            var row = (int)MathF.Round((y - bounds.Bottom) / rowSpacing);
            var offset = row % 2 == 0 ? 0f : spacing / 2f;

            for (var x = bounds.Left + offset; x <= bounds.Right; x += spacing)
            {
                xIter++;

                var localPos = new Vector2i((int)x, (int)y); // they need to kill me dawg
                var worldPos = new EntityCoordinates(gridEnt, localPos);
                Log.Info($"Spawning marker: worldpos X={worldPos.X} Y={worldPos.Y}");

                //var marker = SpawnAtPosition(ent.Comp.MarkerPrototype, worldPos);
                var marker = SpawnAttachedTo(ent.Comp.MarkerPrototype, worldPos);
                Log.Info($"Spawned marker {marker} with parent {_transform.GetParentUid(marker)}, should be attached to {gridEnt}");

                // TODO: just nuke any hexes off-station this parenting shit is evil

                //_transform.SetParent(marker, ent);
                //Log.Info($"Setting parent to {ent} for marker {marker}. Parent is now {_transform.GetParentUid(marker)}");

                if (TryComp<GlimmerHexMarkerComponent>(marker, out var comp))
                    comp.HexName = $"HEX {yIter}-{xIter}"; // TODO: yIter -> greek alphabet
            }
        }
    }
}
