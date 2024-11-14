using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong ClientId;
    public FixedString128Bytes PlayerId;
    public int SpriteIndex;

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId && PlayerId == other.PlayerId && SpriteIndex == other.SpriteIndex;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerId);
        serializer.SerializeValue(ref SpriteIndex);
    }
}
