using Unity.Netcode;

/// <summary>
/// Network-serializable wrapper for a SyncRecord.
/// Sent over the wire via RPCs.
/// </summary>
public struct RecordPayload : INetworkSerializable
{
    public Unity.Collections.FixedString128Bytes Guid;
    public Unity.Collections.FixedString64Bytes  Origin;
    public Unity.Collections.FixedString128Bytes Key;
    public Unity.Collections.FixedString512Bytes Value;
    public Unity.Collections.FixedString64Bytes  Timestamp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Guid);
        serializer.SerializeValue(ref Origin);
        serializer.SerializeValue(ref Key);
        serializer.SerializeValue(ref Value);
        serializer.SerializeValue(ref Timestamp);
    }

    public SyncRecord ToSyncRecord() => new SyncRecord
    {
        Guid      = Guid.ToString(),
        Origin    = Origin.ToString(),
        Key       = Key.ToString(),
        Value     = Value.ToString(),
        Timestamp = Timestamp.ToString()
    };

    public static RecordPayload FromSyncRecord(SyncRecord r) => new RecordPayload
    {
        Guid      = r.Guid,
        Origin    = r.Origin,
        Key       = r.Key,
        Value     = r.Value,
        Timestamp = r.Timestamp
    };
}
