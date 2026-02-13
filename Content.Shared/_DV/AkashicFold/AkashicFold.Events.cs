using Robust.Shared.Serialization;

namespace Content.Shared._DV.AkashicFold;

// This is communicated to the client so they can perform a variety of functions on send.
[Serializable, NetSerializable]
public sealed class SentToFoldEvent : EntityEventArgs
{
    public TimeSpan? Duration { get; }

    public SentToFoldEvent(TimeSpan? duration)
    {
        Duration = duration;
    }
}
