using Content.Server._DV.AkashicFold;
using Content.Server._DV.StationEvents.Components;
using Content.Server.Mind;
using Content.Server.Psionics;
using Content.Server.StationEvents.Events;
using Content.Shared._DV.AkashicFold;
using Content.Shared.Abilities.Psionics;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server._DV.StationEvents.Events;

public sealed class RandomFoldTransportRule : StationEventSystem<RandomFoldTransportRuleComponent>
{

    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobstate = default!;
    [Dependency] private readonly FoldTransportSystem _foldTransport = default!;

    protected override void Started(EntityUid uid,
        RandomFoldTransportRuleComponent comp,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, comp, gameRule, args);

        var targetList = new List<Entity<ActorComponent>>();
        var query = EntityQueryEnumerator<ActorComponent>(); // NOTE: replace this with only Psionics
        Log.Info("Starting fold transport event...");
        while (query.MoveNext(out var ent, out var actor))
        {
            if (!_mobstate.IsAlive(ent) || HasComp<PsionicInsulationComponent>(ent) || HasComp<InFoldComponent>(ent)) // reconsider IsAlive check
            {
                Log.Info("Entity " + Name(ent) + " failed the Gamer Test:tm:");
                continue;
            }


            targetList.Add((ent, actor));
            Log.Info("Entity " + Name(ent) + " added to target list");
        }

        if (targetList.Count == 0)
            Log.Info("erm.... no targets... what the scallop?");

        var toTransport = _random.Next(comp.MinimumTargets, comp.MaximumTargets);

        for (var i = 0; i < toTransport && targetList.Count > 0; i++)
        {
            var transportTarget = _random.PickAndTake(targetList);
            Log.Info("We have a transport target: " + Name(transportTarget));
            //WAUGH.... SPOOKY CODE HERE... SPOOOOOOOKY...
            var time = _random.Next(comp.MinimumTime, comp.MaximumTime);
            _foldTransport.TransportActorToFold(transportTarget, time);
        }
    }
}
