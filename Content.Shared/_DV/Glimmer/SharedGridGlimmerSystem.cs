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
public abstract class SharedGridGlimmerSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    //private EntityQuery<GlimmerHexMarkerComponent> _hexQuery;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        //_hexQuery = GetEntityQuery<GlimmerHexMarkerComponent>();
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
}
