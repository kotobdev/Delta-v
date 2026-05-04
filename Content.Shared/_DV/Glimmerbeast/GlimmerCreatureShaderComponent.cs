namespace Content.Shared._DV.Glimmerbeast;

/// <summary>
/// This is used for making fascinating creatures look fascinating.
/// </summary>
[RegisterComponent]
public sealed partial class GlimmerCreatureShaderComponent : Component
{
    /// <summary>
    ///     Localization string for how you'd like to describe this effect.
    /// </summary>
    [DataField]
    public string? ExaminedDesc;
}
