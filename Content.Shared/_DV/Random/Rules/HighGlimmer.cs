using Content.Shared.Psionics.Glimmer;
using Content.Shared.Random.Rules;

namespace Content.Shared._DV.Random.Rules;

public sealed partial class HighGlimmerRule : RulesRule
{
    [DataField]
    public int Threshold = 700;

    public override bool Check(EntityManager entManager, EntityUid uid)
    {
        var glimmerSys = entManager.System<GlimmerSystem>();

        if(glimmerSys.Glimmer >= Threshold)
            return true;
        return false;
    }
}
