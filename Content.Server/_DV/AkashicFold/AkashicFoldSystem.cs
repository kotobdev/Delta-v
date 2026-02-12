using System.Linq;
using System.Numerics;
using Content.Server._DV.Planet;
using Content.Server.GameTicking.Events;
using Content.Server.Parallax;
using Content.Shared._DV.AkashicFold;
using Content.Shared._DV.Planet;
using Content.Shared.Light.Components;
using Content.Shared.Parallax.Biomes;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._DV.AkashicFold;

public sealed class AkashicFoldSystem : EntitySystem
{
    [Dependency] private readonly BiomeSystem _biome = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly PlanetSystem _planet = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    private readonly ProtoId<PlanetPrototype> _foldPlanet = "AkashicFold";
    private readonly ResPath _baseGridPath = new("Maps/_DV/AkashicFold/akashic_base.yml");
    private static EntityUid? _mapEntUid;
    private static MapId _mapId;
    private static float _scalingFactor = 10f; // for the love of god move all this shit to a comp
    private static Vector2 _realworldCenter = new(0, 0); // FOR THE LOVE OF GOD MOVE ALL THIS SHIT TO A COMP

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartingEvent>(OnRoundStart);
    }

    private void OnRoundStart(RoundStartingEvent ev)
    {
        LoadFold();
    }

    private void LoadFold()
    {
        var map = _planet.SpawnPlanet(_foldPlanet, runMapInit: false);
        _mapEntUid = map;
        _mapId = Comp<MapComponent>(_mapEntUid.Value).MapId;

        LoadFoldGrid(_baseGridPath, new Vector2i(0, 0)); // base camp, always centered
        Log.Info("hiiii helloo :3");
        PlaceRuins();

        _map.InitializeMap(map);
    }

    // placeholder until smarter ruin placement is done
    private void PlaceRuins()
    {
        if (_mapEntUid == null)
            return;

        var ruins = _proto.EnumeratePrototypes<AkashicRuinPrototype>().ToList();

        if (ruins.Count == 0) // no ruins available
        {
            Log.Warning("no ruins available");
            return;
        }


        var query = EntityQueryEnumerator<TransformComponent, AkashicPoiIndicatorComponent>();

        List<Vector2> poiIndicators = new();

        while (query.MoveNext(out var ent, out var transform, out var indicator))
        {
            Log.Info("indicator FOUND: " + ent);
            poiIndicators.Add(_transform.GetMapCoordinates(transform).Position);
        }

        if (poiIndicators.Count == 0)
            Log.Info("ermmm no indicators...");

        // evil gamer math to align the locations so (0,0) is the center of all of them
        var center = Vector2.Zero;
        foreach (var v in poiIndicators)
        {
            center += v;
        }
        center /= poiIndicators.Count;

        _realworldCenter = center;
        Log.Info("Akashic Fold realspace center coords: " + _realworldCenter);

        for (var i = 0; i < poiIndicators.Count; i++)
        {
            poiIndicators[i] -= center;
            Log.Info("AKASHIC POI UUUH " + poiIndicators[i]);
        }
    }

    private bool LoadGridRuin(AkashicRuinPrototype ruin, Vector2i coords)
    {
        if(!(LoadFoldGrid(ruin.MapPath, coords) is { } ruinGrid))
            return false;

        if(!ruin.RoofEnabled)
            RemComp<ImplicitRoofComponent>(ruinGrid);

        return true;
    }

    private Entity<MapGridComponent>? LoadFoldGrid(ResPath path, Vector2i coords)
    {
        if (_mapEntUid == null)
            return null;

        if (!_mapLoader.TryLoadGrid(_mapId, path, out var spawnedBoundedGrid))
        {
            Log.Error($"Failed to load Fold grid {path.Filename}!");
            return null;
        }

        var spawned = spawnedBoundedGrid.Value;
        _transform.SetCoordinates(spawned, new EntityCoordinates(_mapEntUid.Value, coords));
        _biome.ReserveTiles(_mapEntUid.Value,
            Comp<MapGridComponent>(spawned).LocalAABB,
            new List<(Vector2i, Tile)>(),
            Comp<BiomeComponent>(_mapEntUid.Value),
            Comp<MapGridComponent>(_mapEntUid.Value));

        return spawned;
    }

    // this entire file sucks, make this return a MapCoordinates i think, then remove GetMapId probably
    public Vector2 RealToFoldCoordinates(Vector2i coords)
    {
        return (coords - _realworldCenter) * _scalingFactor;
    }

    public MapId GetMapId()
    {
        //FOR THE LOVE OF GOD!!! COMPONENT!!! PLEASE!!!
        return _mapId;
    }
}
