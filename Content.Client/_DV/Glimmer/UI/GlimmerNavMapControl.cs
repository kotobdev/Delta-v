using System.Numerics;
using Content.Client.Pinpointer.UI;
using Content.Shared._DV.Glimmer;
using Content.Shared.Prototypes;
using Microsoft.VisualBasic.CompilerServices;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Client._DV.Glimmer.UI;

public sealed class GlimmerNavMapControl : NavMapControl
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private readonly SpriteSystem _spriteSystem;

    private static readonly ProtoId<NavMapBlipPrototype> GlimmerHexBlip = "GlimmerHex";

    private Dictionary<NetEntity, GlimmerHexBlip>? _lastHexes;

    public GlimmerNavMapControl()
    {
        IoCManager.InjectDependencies(this);

        _spriteSystem = _entManager.System<SpriteSystem>();
    }

    protected override void UpdateNavMap()
    {
        base.UpdateNavMap();

        if (MapUid is not { } mapUid ||
            !_entManager.TryGetComponent<GlimmerHexNavMapComponent>(mapUid, out var mapComp))
            return;

        _lastHexes = mapComp.Hexes;
        RebuildBlips(mapUid, mapComp.Hexes);
    }

    private void RebuildBlips(EntityUid mapUid, Dictionary<NetEntity, GlimmerHexBlip> hexes)
    {
        TrackedEntities.Clear();

        if (!_proto.TryIndex(GlimmerHexBlip, out var proto) || proto.TexturePaths is not { Length: > 0 } paths)
            return;

        var texture = _spriteSystem.Frame0(new SpriteSpecifier.Texture(paths[0]));

        foreach (var (netEntity, hex) in hexes)
        {
            var coords = new EntityCoordinates(mapUid, hex.Position);
            var color = GlimmerColorLerp(hex.Glimmer, Color.Black, proto.Color); // TODO: less sloppy dynamic color system with lerp

            TrackedEntities[netEntity] =
                new NavMapBlip(coords, texture, color, proto.Blinks, proto.Selectable, proto.Scale);
        }
    }

    private Color GlimmerColorLerp(int glimmer, Color baseColor, Color targetColor)
    {
        var t = Math.Clamp(glimmer / 1000f, 0f, 1f);

        // i feel horrible.
        return new Color(
            MathHelper.Lerp(baseColor.R, targetColor.R, t),
            MathHelper.Lerp(baseColor.G, targetColor.G, t),
            MathHelper.Lerp(baseColor.B, targetColor.B, t),
            MathHelper.Lerp(baseColor.A, targetColor.A, t));
    }

}
