using Content.Shared.Drunk;
using Content.Shared.EntityEffects;
using Content.Shared.StatusEffect;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
///     Reduces drunk time by dividing the current drunk time by a factor.
///     More effective against high levels of drunkedness compared to fixed time removal.
/// </summary>
[UsedImplicitly]
public sealed partial class ReduceDrunkTimeByDivision : EntityEffect
{
    /// <summary>
    ///     Factor to divide the current drunk time by. Higher values mean faster recovery.
    ///     For example, a factor of 2.0 will halve the remaining drunk time.
    /// </summary>
    [DataField]
    public float DivisionFactor = 2.0f;

    /// <summary>
    ///     Minimum time to remove, in seconds. Ensures the effect still works for low drunk levels.
    /// </summary>
    [DataField]
    public float MinimumTimeRemoved = 6.0f;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-reduce-drunk-time-division", 
            ("chance", Probability), 
            ("factor", DivisionFactor),
            ("minimum", MinimumTimeRemoved));

    public override void Effect(EntityEffectBaseArgs args)
    {
        var statusSys = args.EntityManager.EntitySysManager.GetEntitySystem<StatusEffectsSystem>();
        var drunkSys = args.EntityManager.EntitySysManager.GetEntitySystem<SharedDrunkSystem>();

        // Get current drunk time remaining
        if (!statusSys.TryGetTime(args.TargetEntity, SharedDrunkSystem.DrunkKey, out var currentTime))
            return; // Not drunk, nothing to do

        var currentTimeSeconds = currentTime.TotalSeconds;
        var divisionFactor = DivisionFactor;
        var minimumTimeRemoved = MinimumTimeRemoved;

        if (args is EntityEffectReagentArgs reagentArgs)
        {
            divisionFactor *= reagentArgs.Scale.Float();
            minimumTimeRemoved *= reagentArgs.Scale.Float();
        }

        // Calculate time to remove: either divide current time or use minimum
        var timeToRemoveByDivision = currentTimeSeconds * (1.0 - 1.0 / divisionFactor);
        var timeToRemove = Math.Max(timeToRemoveByDivision, minimumTimeRemoved);

        // Ensure we don't remove more time than available
        timeToRemove = Math.Min(timeToRemove, currentTimeSeconds);

        if (timeToRemove > 0)
        {
            drunkSys.TryRemoveDrunkenessTime(args.TargetEntity, timeToRemove);
        }
    }
}