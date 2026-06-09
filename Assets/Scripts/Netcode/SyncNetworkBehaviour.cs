using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Attached to a NetworkObject that exists on both server and clients.
///
/// Flow:
///   1. Client connects → OnNetworkSpawn fires on client.
///   2. Client reads its local DB and sends all records to the server via ServerRpc.
///   3. Server stores received records in its own DB.
///   4. Server then pushes its own DB records back to that specific client via ClientRpc.
///   5. Client stores received records in its own DB.
///
/// Both directions are therefore covered.
/// </summary>
public class SyncNetworkBehaviour : NetworkBehaviour
{
    // Injected by SyncManager after spawn
    public NetcodeDB DB { get; set; }

    // UI callback — fires on main thread
    public System.Action<string> OnLogMessage;
    public System.Action         OnDataChanged;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    public override void OnNetworkSpawn()
    {
        if (IsClient && !IsServer)
        {
            Log("Connected to server. Sending local DB records...");
            SendLocalDBToServer();
        }

        if (IsServer)
        {
            Log("Server ready. Waiting for clients...");
        }
    }

    // ── CLIENT → SERVER ───────────────────────────────────────────────────────

    /// <summary>Client reads its DB and sends each record to the server.</summary>
    private void SendLocalDBToServer()
    {
        if (DB == null) return;
        var records = DB.GetAll();
        Log($"Sending {records.Count} record(s) to server...");
        foreach (var r in records)
            SubmitRecordServerRpc(RecordPayload.FromSyncRecord(r));

        // Signal end of batch so server knows to push back
        BatchCompleteServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitRecordServerRpc(RecordPayload payload, ServerRpcParams rpcParams = default)
    {
        if (DB == null) return;
        var record = payload.ToSyncRecord();
        bool isNew = DB.InsertIfNew(record);
        Log($"[Server] Received from client {rpcParams.Receive.SenderClientId}: " +
            $"{record.Key}={record.Value} ({(isNew ? "NEW" : "duplicate")})");
        OnDataChanged?.Invoke();
    }

    /// <summary>
    /// Client signals it has finished sending its batch.
    /// Server responds by pushing its own DB to that client.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void BatchCompleteServerRpc(ServerRpcParams rpcParams = default)
    {
        if (DB == null) return;
        ulong clientId = rpcParams.Receive.SenderClientId;
        var records = DB.GetAll();
        Log($"[Server] Pushing {records.Count} record(s) to client {clientId}...");

        var target = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { clientId }
            }
        };

        foreach (var r in records)
            PushRecordToClientRpc(RecordPayload.FromSyncRecord(r), target);

        PushCompleteClientRpc(target);
    }

    // ── SERVER → CLIENT ───────────────────────────────────────────────────────

    [ClientRpc]
    private void PushRecordToClientRpc(RecordPayload payload, ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;   // host mode: skip self
        if (DB == null) return;
        var record = payload.ToSyncRecord();
        bool isNew = DB.InsertIfNew(record);
        Log($"[Client] Received from server: {record.Key}={record.Value} ({(isNew ? "NEW" : "duplicate")})");
        OnDataChanged?.Invoke();
    }

    [ClientRpc]
    private void PushCompleteClientRpc(ClientRpcParams rpcParams = default)
    {
        if (IsServer) return;
        Log("[Client] Sync complete. All server records received.");
        OnDataChanged?.Invoke();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void Log(string msg)
    {
        Debug.Log($"[Sync] {msg}");
        OnLogMessage?.Invoke(msg);
    }
}
