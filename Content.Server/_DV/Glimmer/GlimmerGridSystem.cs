using Content.Server.GameTicking.Events;
using Content.Server.Station.Events;
using Content.Shared._DV.Glimmer;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._DV.Glimmer;

/// <inheritdoc/>
public sealed class GlimmerGridSystem : SharedGridGlimmerSystem
{
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly SharedStationSystem _station = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        // I'm tired, boss. This is the 5th event type I've tried.
        SubscribeLocalEvent<GlimmerComponent, StationPostInitEvent>(OnStationPostInit);
    }

    private void OnStationPostInit(Entity<GlimmerComponent> ent, ref StationPostInitEvent args)
    {
        //oh boy i can't way to shitcode my way into this

        if (!TryComp<StationDataComponent>(ent, out var stationData))
            return;

        if (_station.GetLargestGrid((ent, stationData)) is not { } gridEnt) // i dont even know
            return;

        if (!TryComp<MapGridComponent>(gridEnt, out var mapGrid))
            return;

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

                // SO! APPARENTLY! IF YOU DO THIS IN SHARED IT JUST ENDS UP CLIENTSIDED???
                // DAWG?
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
