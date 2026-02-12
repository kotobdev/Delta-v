using Content.Shared._DV.AkashicFold;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Client._DV.AkashicFold;

/// <summary>
/// This handles some of the client-specific fx for sending people to the Fold.
/// </summary>
public sealed class FoldTransportSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly FoldTextDisplaySystem _foldTextDisplay = default!;

    // NOTE: hey, gamer! you're in client now! go mute ambient audio for the duration, please!

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // yeah this probably plays for people who Aren't the primary gamer so maybe make it Not Do That
        var query = EntityQueryEnumerator<FoldTransportedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.EnterFoldTime != null) // this runs every tick up until the transport happens
            {
                //I mean, shit, this is one way to do it
                var rand = _random.NextFloat();
                if (rand > 0.995f) // don't do this
                {
                    Log.Info("HI GAMER!!! HIIII!!!!! RENDERING THE THING NOW GAMER!!!!!");
                    var textPos = _random.NextVector2();
                    _foldTextDisplay.DisplayFoldText("HELLO GAMER!!! HI!!!!", textPos, 3f, 10f);
                }
            }
        }
    }
}
