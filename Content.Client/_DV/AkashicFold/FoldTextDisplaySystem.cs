using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Map;

namespace Content.Client._DV.AkashicFold;

/// <summary>
/// System for the Fancy Fold Text. Pretty similar to the PopupSystem, just substantially cut down
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

    // TODO: fix this abysmal shitcode
    // TODO 2: make a less evil constructor
    public void DisplayFoldText(string message, Vector2 coords, float time, float size, int subLabels)
    {
        var label = new FoldTextLabel
        {
            Position = coords,
            Text = message,
            Time = time,
            Size = size,
        };

        if (subLabels > 0)
        {
            List<FoldTextLabel> labelList = [];
            for (var i = 0; i < subLabels; i++)
            {
                var offset = (i+1f)/(subLabels+1f);
                Log.Info("evil ass offset value: " + offset);
                labelList.Add(new FoldTextLabel // sobs
                {
                    Position = label.Position,
                    Text = label.Text,
                    Time = label.Time,
                    Size = label.Size,
                    AnimationOffset = offset,
                });
            }

            label.SubLabels = labelList;
        }

        Log.Info("We have... " + label.SubLabels.Count + " sublabels...");

        Labels.Add(label);
    }

    public void RemoveAllLabels()
    {
        Labels.Clear();
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

    // TODO: this is definitely not the ECS way to do this
    public sealed class FoldTextLabel
    {
        public Vector2 Position;
        public string Text = string.Empty;
        public float Time = 3f; // How long this should appear
        public float TotalTime; // Counter variable to track how long this specific label has been alive
        public float Size = 1f; //:idk: make this sensible
        public float Opacity = 1f;
        public float AnimationOffset = 0f;
        public List<FoldTextLabel> SubLabels = new();
    }

    /*public sealed class FoldTextLabel : AbstractFoldTextLabel { }

    public sealed class FoldTextSubLabel : AbstractFoldTextLabel
    {

    }*/
}
