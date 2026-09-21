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

    /// <summary>
    /// Adds a distance-relative amount of glimmer to hexes in a radius around the given entity.
    /// If no hexes are in radius, the closest hex will be used.
    /// </summary>
    /// <param name="ent">Entity to use as glimmer source</param>
    /// <param name="amount">How much total glimmer to add. This will be split among all hexes within radius, weighted by distance.</param>
    /// <param name="radius">The radius to check for hexes in.</param>
    public void AddGlimmer(EntityUid? ent, float amount, float radius = 15)
    {
        if (ent == null)
            return;

        var sourcePos = _transform.GetWorldPosition(ent.Value);

        var targets = new List<(Entity<GlimmerHexMarkerComponent> marker, float distance)>();
        Entity<GlimmerHexMarkerComponent>? closestMarker = null;
        var closestDistance = float.MaxValue;

        Log.Info("AddGlimmer iterating over targets now...");

        // A lot of this file functions on the assumption that an EntityQueryEnumerator is going to be
        // faster then lookup in radius. I don't know if this is true, but it feels like it would be.
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
            var weight = 1f / MathF.Max(distance, 0.01f); //0.01 avoid divide by 0
            totalWeight += weight;
        }

        foreach (var (marker, distance) in targets)
        {
            var weight = (1f / MathF.Max(distance, 0.01f)) / totalWeight;
            marker.Comp.Glimmer += (int)(amount * weight); // i love casting to ints!! i love casting to ints!!!
            Log.Info($"Added {amount * weight} glimmer to {marker.Comp.HexName} (total: {marker.Comp.Glimmer}). Distance was {distance}, weight was {weight}");
        }
    }

    /// <summary>
    /// Gets a vague glimmer level around an entity.
    /// </summary>
    /// <param name="ent">Entity to get glimmer level at the position of.</param>
    /// <remarks>
    /// This will need to be adjusted a lot. This value should not be strictly relied on for... anything, really,
    /// with its current implementation. It doesn't make much sense, but then again, does it really have to?
    /// ...It probably has to. God, I don't want to rework this.
    /// </remarks>
    public float GetGlimmer(EntityUid? ent)
    {
        var total = 0f;

        // minimum radius to cover all spots is roughly 12 given a spacing of 20 units
        // however then you'd have weird spots with zero glimmer, and we want glimmer to like. exist. everywhere?
        // look the math afterwards keeps things reasonable anyways
        var radius = 20f; //TODO: DEAR GOD PUT THIS IN MARKER COMP

        Log.Info("uhmmmm... getting glimmer...");

        if (ent == null)
            return total; // should probably have a case for getting "global" glimmer as a fallback

        Log.Info("passed null check");

        var query = EntityQueryEnumerator<GlimmerHexMarkerComponent>();
        while (query.MoveNext(out var marker, out var comp))
        {
            var pos = Transform(marker).Coordinates;

            var distance = Vector2.Distance(_transform.GetWorldPosition(ent.Value), _transform.GetWorldPosition(marker));

            Log.Info($"GetGlimmer iterating over marker {comp.HexName} at distance {distance}");

            var t = distance / radius;

            if (t > 1f)
                continue;

            var falloff = 1f - t;
            falloff *= falloff;

            Log.Info($"{comp.HexName} now adding {comp.Glimmer * falloff} glimmer (base: {comp.Glimmer}, falloff: {falloff})");

            total += comp.Glimmer * falloff;
        }

        return total;
    }
}
