using Robust.Shared.Serialization;

namespace Content.Shared._DV.AkashicFold;

// This is communicated to the client so they can clear the flavor text on send.
[Serializable, NetSerializable]
public sealed class SentToFoldEvent : EntityEventArgs
{

}
