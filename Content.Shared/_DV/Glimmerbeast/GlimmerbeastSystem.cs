using Content.Shared.EntityEffects.Effects.Botany.PlantAttributes;
using Content.Shared.Examine;
using Content.Shared.Humanoid;
using Content.Shared.IdentityManagement.Components;
using Content.Shared.Mobs;
using Content.Shared.Popups;
using Robust.Shared.Enums;

namespace Content.Shared._DV.Glimmerbeast;

public sealed class GlimmerbeastSystem : EntitySystem
{

    [Dependency] private readonly SharedHumanoidAppearanceSystem _humanoidAppearance = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GlimmerbeastComponent, ComponentStartup>(OnCompStart);
        SubscribeLocalEvent<GlimmerbeastComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<GlimmerbeastComponent, ExamineAttemptEvent>(OnExamineAttempt);
        SubscribeLocalEvent<GlimmerbeastComponent, SeeIdentityAttemptEvent>(OnSeeIdentityAttempt);
        SubscribeLocalEvent<GlimmerbeastComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnCompStart(Entity<GlimmerbeastComponent> ent, ref ComponentStartup args)
    {
        // look, apparently you cannot do this via yaml. Gets overridden by the species bit, I presume.
        if(!TryComp<HumanoidAppearanceComponent>(ent, out var humanoidAppearance))
            return;

        _humanoidAppearance.SetGender((ent, humanoidAppearance), Gender.Neuter);
        _metaData.SetEntityName(ent, Loc.GetString(ent.Comp.NameOverride));
    }

    private void OnExamineAttempt(Entity<GlimmerbeastComponent> ent, ref ExamineAttemptEvent args)
    {
        //if(!TryComp<GlimmerbeastComponent>(args.Examiner, out _))
        //    args.Cancel();
    }

    private void OnExamined(Entity<GlimmerbeastComponent> ent, ref ExaminedEvent args)
    {
        args.PushMarkup(Loc.GetString(ent.Comp.ExaminedDesc));
    }

    private void OnSeeIdentityAttempt(Entity<GlimmerbeastComponent> ent, ref SeeIdentityAttemptEvent args)
    {
        // Ideally this would be different if the observer is a glimmerbeast as well
        // however, that would take a surprising amount of work.
        args.NameOverride = Loc.GetString(ent.Comp.NameOverride);
    }

    private void OnMobStateChanged(Entity<GlimmerbeastComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var entCoords = Transform(ent).Coordinates;

        if (ent.Comp.DespawnVfx != null)
            PredictedSpawnAtPosition(ent.Comp.DespawnVfx, entCoords);

        _popup.PopupPredictedCoordinates(Loc.GetString(ent.Comp.DeathPopupText), entCoords, ent, PopupType.Medium);

        // TODO: Once whiteout gameplay flow is established,
        // a ghostrole should be opened when deleting.
        PredictedQueueDel(ent);
    }
}
