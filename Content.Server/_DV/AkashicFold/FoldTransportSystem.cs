using System.Numerics;
using Content.Server.Cloning;
using Content.Server.Jittering;
using Content.Server.Mind;
using Content.Server.Stunnable;
using Content.Shared._DV.AkashicFold;
using Content.Shared.Cloning;
using Content.Shared.Jittering;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
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
/// This handles entities getting sent to and from the fold.
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
    [Dependency] private readonly EyeSystem _eye = default!;
    [Dependency] private readonly SharedContentEyeSystem _contentEye = default!;

    public readonly ProtoId<CloningSettingsPrototype> SettingsId = "CloningPod";
    //private TimeSpan _warningSoundLength;
    //private ResolvedSoundSpecifier _resolvedWarningSound = String.Empty;

    /// <inheritdoc/>
    /*public override void Initialize()
    {

    }*/

    public void PrepareTransportToFold(Entity<ActorComponent> ent, TimeSpan time)
    {
        if (!TryComp<MindContainerComponent>(ent, out var mindContainer) || !mindContainer.HasMind)
        {
            Log.Info("Transport target failed MindContainer check: " + Name(ent));
            return;
        }

        if (TryComp<InFoldComponent>(ent, out _)) // redundant check, this would be VERY bad.
            return;

        // refresh duration if this entity is ALREADY in the fold
        // reevaluate since this could cause issues with the audio
        if (TryComp<FoldTransportedComponent>(ent, out var comp))
        {
            comp.ExitFoldTime = _timing.CurTime + time;
            return;
        }
        var foldTransported = AddComp<FoldTransportedComponent>(ent);

        var resolvedWarningSound = _audio.ResolveSound(foldTransported.EnterWarningSound);
        var warningSoundLength = _audio.GetAudioLength(resolvedWarningSound);
        foldTransported.EnterFoldTime = _timing.CurTime + warningSoundLength;
        foldTransported.ExitFoldTime = _timing.CurTime + time;
        foldTransported.ExitFoldAudioTime = foldTransported.ExitFoldTime - warningSoundLength; // i am beautiful.
        Dirty(ent, foldTransported); // client will need this data, 99% sure that means we need to dirty it here

        _audio.PlayEntity(foldTransported.EnterWarningSound, ent, ent);

        // MOVE ALL LOGIC BELOW THIS LINE TO ITS OWN FUNCTION, CALLED IN UPDATE LOOP!!!
    }

    private void TransportToFold(Entity<ActorComponent> ent, FoldTransportedComponent foldTransported)
    {
        if (!TryComp<MindContainerComponent>(ent, out var mindContainer) || !mindContainer.HasMind)
            return; // i love redundancy

        var foldSpawnCoords = new MapCoordinates(_transform.GetMapCoordinates(ent).Position, _akashicFold.GetMapId());
        if (!_cloning.TryCloning(ent, foldSpawnCoords, SettingsId, out var clone))
        {
            Log.Info("Fold cloning FAILED for entity: " + Name(ent));
            return;
        }

        // make sure this happens BEFORE transfer
        var ev = new SentToFoldEvent();
        RaiseNetworkEvent(ev, ent.Comp.PlayerSession);

        var mindEnt = mindContainer.Mind.Value;
        _mind.TransferTo(mindEnt, clone);

        AddComp<InFoldComponent>(clone.Value);

        foldTransported.FoldBody = clone.Value;

        var audioPlayer = _audio.PlayGlobal(foldTransported.TransportedAmbientTrack, Filter.Entities(clone.Value), false);
        if(audioPlayer != null)
            foldTransported.AmbiSoundStream = audioPlayer.Value.Entity;

        //flavor
        _stun.TryKnockdown((ent, null), null, true, false); // okay this cannot be the right way to invoke this

        if (foldTransported.ExitFoldTime == null) // i love nullables
            return;

        _jitter.DoJitter(ent, foldTransported.ExitFoldTime.Value - _timing.CurTime, true, 2f, 1f);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var transportedQuery = EntityQueryEnumerator<FoldTransportedComponent, ContentEyeComponent>();
        while (transportedQuery.MoveNext(out var uid, out var comp, out var eye)) // players have eyes right
        {
            if (comp.EnterFoldTime != null) // this runs every tick up until the transport happens
            {
                //_eye.SetZoom(uid, eye.Zoom * 0.95f, eye); // slow zoom in... also mult by frametime nerd
                //_eye.UpdateEye((uid, eye)); // why does it not refresh? might have to do this trick on Client
                //Dirty(uid, eye); // surely this can only end well
                _contentEye.SetZoom(uid, eye.TargetZoom * MathF.Pow(0.87f, frameTime)); // shrugtuah!
            }

            if (comp.ExitFoldAudioTime != null && _timing.CurTime >= comp.ExitFoldAudioTime)
            {
                _audio.PlayEntity(comp.EnterWarningSound, comp.FoldBody, comp.FoldBody);
                comp.ExitFoldAudioTime = null;
            }

            if (comp.EnterFoldTime != null && _timing.CurTime >= comp.EnterFoldTime)
            {
                if (!TryComp<ActorComponent>(uid, out var actor))
                    continue;

                TransportToFold((uid, actor), comp);
                comp.EnterFoldTime = null;
            }

            if (_timing.CurTime >= comp.ExitFoldTime)
            {
                Log.Info(Name(uid) + " is exiting the fold...");
                if (!_mind.TryGetMind(comp.FoldBody, out var mindEnt, out var mind))
                {
                    Log.Warning("Failed to get mind for Fold body " + Name(comp.FoldBody) + ". Something may be very wrong.");
                    continue;
                }

                _audio.Stop(comp.AmbiSoundStream);

                mind.PreventGhosting = false;
                _mind.TransferTo(mindEnt, uid);
                QueueDel(comp.FoldBody);
                RemCompDeferred<FoldTransportedComponent>(uid);

                // flavor!
                _stun.TryKnockdown((uid, null), TimeSpan.FromSeconds(2f), true, true);
                _jitter.DoJitter(uid, TimeSpan.FromSeconds(7f), true, 0.3f, 12f);
            }
        }
    }
}
