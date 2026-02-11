namespace Content.Server._DV.StationEvents.Components;

[RegisterComponent]
public sealed partial class RandomFoldTransportRuleComponent : Component
{
    [DataField]
    public TimeSpan MinimumTime = TimeSpan.FromSeconds(25);

    [DataField]
    public TimeSpan MaximumTime = TimeSpan.FromSeconds(50);

    [DataField]
    public int MinimumTargets = 1;

    [DataField]
    public int MaximumTargets = 1;
}
