using Content.Client.Popups;
using Content.Client.Resources;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Shared.Random;

namespace Content.Client._DV.AkashicFold;

/// <summary>
/// This handles drawing Fancy Fold Text:tm:, essentially really cool looking popups
/// </summary>
public sealed class FoldTextOverlay : Overlay
{
    [Dependency] private IResourceCache _cache = default!;
    [Dependency] private IRobustRandom _random = default!;
    private readonly FoldTextDisplaySystem _foldText; // if this doesn't work: do it the way PopupOverlay does

    private Font _font;

    public FoldTextOverlay(FoldTextDisplaySystem foldText)
    {
        IoCManager.InjectDependencies(this);

        _foldText = foldText; // i think this has to be done for non-interface things? i really have no idea, i just know Cache works but not this

        // this maaaaay need to be passed into the label objects
        // if we want multiple sizes...
        _font = _cache.GetFont("/Fonts/Boxfont-round/Boxfont Round.ttf", 25);
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        if (_foldText.Labels.Count == 0 || args.ViewportControl == null)
            return;

        foreach (var label in _foldText.Labels)
        {
            var pos = (args.ViewportBounds.Size * 0.5f) + label.InitialPos * (args.ViewportBounds.Size * 0.5f); // excuse me?
            var alpha = MathF.Min(1f, 1f - MathF.Max(0f, label.TotalTime - label.Time / 2) * 2 / label.Time); // stolen from PopupUIController

            // draw the sub text before the main text
            var rand = _random.NextFloat();
            if (rand > 0.7f)
            {
                // maybe move this to sub-text objects so it's not. like. single-frame and framerate dependent like this
                args.ScreenHandle.DrawString(_font, pos + _random.NextVector2(30f), label.Text, 1f, Color.White.WithAlpha(0.2f));
            }

            // now, the main text
            args.ScreenHandle.DrawString(_font, pos, label.Text, 1f, Color.White.WithAlpha(alpha)); // this position may be Fucked Up, Dude
        }
    }
}
