using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client._DV.AkashicFold;

/// <summary>
/// This handles...
/// </summary>
public sealed class FoldTextDisplaySystem : EntitySystem
{

    [Dependency] private readonly IOverlayManager _overlay = default!;

    public List<FoldTextLabel> Labels = new();

    public override void Initialize()
    {
        //look, not sure if it's a good idea to add overlay here, but...
        _overlay.AddOverlay(new FoldTextOverlay(this));
    }

    public void DisplayFoldText(string message, Vector2 coords, float time, float size)
    {


        var label = new FoldTextLabel
        {
            InitialPos = coords,
            Text = message,
            Time = time,
            Size = size,
        };

        Labels.Add(label);
    }

    public override void FrameUpdate(float frameTime)
    {
        if (Labels.Count == 0)
            return;

        var labelToRemove = new List<FoldTextLabel>(); // can't modify lists in place!!
        foreach (var label in Labels)
        {
            label.TotalTime += frameTime;
            if (label.TotalTime > label.Time)
            {
                labelToRemove.Add(label);
            }
        }

        foreach (var label in labelToRemove)
        {
            Labels.Remove(label);
        }
    }

    public sealed class FoldTextLabel
    {
        public Vector2 InitialPos;
        public string Text = string.Empty;
        public float Time = 3f; // How long this should appear
        public float TotalTime; // Counter variable to track how long this specific label has been alive
        public float Size = 1f; //:idk: make this sensible
        public float Opacity = 1f;
    }
}
