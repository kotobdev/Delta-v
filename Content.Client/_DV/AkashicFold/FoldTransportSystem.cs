using System.Numerics;
using Content.Client._DV.Overlays;
using Content.Client.Audio;
using Content.Shared._DV.AkashicFold;
using Content.Shared.Random.Helpers;
using Content.Shared.StatusIcon.Components;
using Robust.Client.Player;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Client._DV.AkashicFold;

/// <summary>
/// This handles some of the client-specific fx for sending people to the Fold.
/// </summary>
public sealed class FoldTransportSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly FoldTextDisplaySystem _foldTextDisplay = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ContentAudioSystem _audio = default!;
    [Dependency] private readonly GlimmerOverlaySystem _glimmerOverlay = default!;

    // NOTE: hey, gamer! you're in client now! go mute ambient audio for the duration, please!

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FoldTransportedComponent, GetStatusIconsEvent>(GetFoldSSDIcon);
        SubscribeLocalEvent<FoldTransportedComponent, ComponentStartup>(OnComponentStartup);

        SubscribeNetworkEvent<SentToFoldEvent>(OnSentToFoldEvent);
    }

    // Essentially signals the start of the whole process without needing a new event
    // okay Nevermind we need a new event
    private void OnComponentStartup(Entity<FoldTransportedComponent> ent, ref ComponentStartup args)
    {
        if (_player.LocalEntity != ent)
            return;

        Log.Info("uh oh...");

        if (ent.Comp.EnterFoldTime == null)
            return;

        Log.Info("starting the glimmer thingy now");
        _glimmerOverlay.FoldTransportEnable(_timing.CurTime, ent.Comp.EnterFoldTime.Value);
    }

    private void GetFoldSSDIcon(Entity<FoldTransportedComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_timing.CurTime < ent.Comp.EnterFoldTime)
            return;

        if(_prototype.Resolve(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void OnSentToFoldEvent(SentToFoldEvent msg)
    {
        Log.Info("hey yeah!! we got the networkevent!! yay!!");
        _foldTextDisplay.RemoveAllLabels();
        if (msg.Duration != null)
        {
            _audio.PauseAmbientMusic(msg.Duration.Value);
        }
    }

    // TODO: collapse this into functions
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        //var query = EntityQueryEnumerator<FoldTransportedComponent>();
        var playerEntity = _player.LocalEntity;

        if (playerEntity == null)
            return;

        if (!TryComp<FoldTransportedComponent>(playerEntity, out var comp))
            return;

        if (comp.EnterFoldTime != null && _timing.CurTime <= comp.EnterFoldTime) // this runs every tick up until the transport happens.
        {
            if (comp.NextFlavorTextTime == null || _timing.CurTime >= comp.NextFlavorTextTime)
            {
                Log.Info("HI GAMER!!! HIIII!!!!! RENDERING THE THING NOW GAMER!!!!!");

                var flavorText = _random.Pick(_prototype.Index(comp.TransportMinorFlavorTexts)); // WE SHOULD NOT BE DOING ALL THIS EVERY FRAME MY GOD

                var textPos = _random.NextVector2();
                // TODO: adjust max to get lower and lower over time
                // TODO 2: check if the alignment code is actually working correctly
                textPos = Vector2.Normalize(textPos) * (0.2f + 0.8f * textPos.Length()); // long-winded way to make text not appear near the very center
                _foldTextDisplay.DisplayFoldText(flavorText, textPos, 3f, 5f, 3);

                // evil code for a continually-decreasing delay
                var t = Math.Clamp((comp.EnterFoldTime - _timing.CurTime).GetValueOrDefault().TotalSeconds / 5.0,
                    0,
                    1);
                Log.Info("T VALUE: " + t);
                t = Math.Pow(t, 3); // exponential easing my beloved
                Log.Info("LERP VALUE: " + t);
                var offset = _timing.CurTime + TimeSpan.FromSeconds(double.Lerp(0.2f, 2f, t));

                comp.NextFlavorTextTime = offset; // god please
            }
        }

        if (_timing.CurTime >= comp.ExitFoldTime)
        {
            Log.Info("HI!! just testing if this gets ran. probably not");
        }
    }
}
