using System.Numerics;
using Content.Server.Cloning;
using Content.Server.Jittering;
using Content.Server.Mind;
using Content.Server.Stunnable;
using Content.Shared._DV.AkashicFold;
using Content.Shared.Cloning;
using Content.Shared.Jittering;
using Content.Shared.Mind.Components;
using Content.Shared.StatusEffectNew;
using Robust.Server.Audio;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._DV.AkashicFold;

/// <summary>
/// This handles entities getting send to and from the fold.
/// Inherently, very bad things happen if we mess this up.
/// </summary>
public sealed class FoldTransportSystem : EntitySystem
{

    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly CloningSystem _cloning = default!;
    [Dependency] private readonly AkashicFoldSystem _akashicFold = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public readonly ProtoId<CloningSettingsPrototype> SettingsId = "CloningPod";
    private TimeSpan _warningSoundLength;
    private ResolvedSoundSpecifier _resolvedWarningSound = String.Empty;

    /// <inheritdoc/>
    /*public override void Initialize()
    {

    }*/

    public void TransportActorToFold(Entity<ActorComponent> ent, TimeSpan time)
    {
        if (!TryComp<MindContainerComponent>(ent, out var mindContainer) || !mindContainer.HasMind)
        {
            Log.Info("Transport target failed MindContainer check: " + Name(ent));
            return;
        }

        if (TryComp<InFoldComponent>(ent, out _)) // redundant check, this would be VERY bad.
            return;

        // refresh duration if this entity is ALREADY in the fold
        if (TryComp<FoldTransportedComponent>(ent, out var comp))
        {
            comp.ExitFoldTime = _timing.CurTime + time;
            return;
        }

        MapCoordinates foldSpawnCoords = new MapCoordinates(_transform.GetMapCoordinates(ent).Position, _akashicFold.GetMapId());
        if (!_cloning.TryCloning(ent, foldSpawnCoords, SettingsId, out var clone))
        {
            Log.Info("Fold cloning FAILED for entity: " + Name(ent));
            return;
        }

        var mindEnt = mindContainer.Mind.Value;
        _mind.TransferTo(mindEnt, clone);

        AddComp<InFoldComponent>(clone.Value);
        var foldTransported = AddComp<FoldTransportedComponent>(ent);
        foldTransported.foldBody = clone.Value;
        foldTransported.ExitFoldTime = _timing.CurTime + time;

        //flavor
        _jitter.DoJitter(ent, time, true, 2f, 1f);
        _stun.TryKnockdown((ent, null), null, true, false); // okay this cannot be the right way to invoke this

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var transportedQuery = EntityQueryEnumerator<FoldTransportedComponent>();
        while (transportedQuery.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.ExitFoldTime)
            {
                Log.Info(Name(uid) + " is exiting the fold...");
                if (!_mind.TryGetMind(comp.foldBody, out var mindEnt, out var mind))
                {
                    Log.Warning("Failed to get mind for Fold body " + Name(comp.foldBody) + ". Something may be very wrong.");
                    continue;
                }

                mind.PreventGhosting = false;
                _mind.TransferTo(mindEnt, uid);
                QueueDel(comp.foldBody);
                RemCompDeferred<FoldTransportedComponent>(uid);
            }
        }
    }
}
