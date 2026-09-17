using Robust.Client.UserInterface;

namespace Content.Client._DV.Glimmer.UI;

public sealed class GlimmerNavMapBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GlimmerNavMapWindow? _window;

    public GlimmerNavMapBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GlimmerNavMapWindow>();

        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var xform))
            _window.SetMapUid(xform.GridUid);
    }
}
