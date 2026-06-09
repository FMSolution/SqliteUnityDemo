using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

/// <summary>
/// Central manager for the Netcode sync demo.
/// Handles starting host/server/client, owns the DB, and spawns the sync object.
/// </summary>
public class SyncManager : MonoBehaviour
{
    public static SyncManager Instance { get; private set; }

    [Header("Network")]
    [Tooltip("IP address to connect to (client) or listen on (server/host).")]
    public string Address = "127.0.0.1";
    public ushort Port    = 7777;

    [Header("Prefab")]
    [Tooltip("Prefab with NetworkObject + SyncNetworkBehaviour.")]
    public GameObject syncPrefab;

    // ── State ─────────────────────────────────────────────────────────────────
    public NetcodeDB DB { get; private set; }
    public bool IsRunning => NetworkManager.Singleton != null &&
                             (NetworkManager.Singleton.IsServer ||
                              NetworkManager.Singleton.IsClient);

    public string Role { get; private set; } = "None";

    // Events → UI
    public event Action<string> OnLog;
    public event Action         OnDataChanged;

    private SyncNetworkBehaviour _syncBehaviour;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        // Open DB — name differs per role so server and client on same machine don't clash
        // Will be renamed after role is chosen
    }

    private void OnDestroy()
    {
        Stop();
        DB?.Close();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartHost(string address, ushort port)
    {
        Role = "Host";
        InitDB("netcode_host.db");
        ConfigureTransport(address, port);

        NetworkManager.Singleton.OnClientConnectedCallback    += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback   += OnClientDisconnected;
        NetworkManager.Singleton.StartHost();

        // Spawn the sync object (server authority)
        SpawnSyncObject();
        Log($"[Host] Started on {address}:{port}");
    }

    public void StartServer(string address, ushort port)
    {
        Role = "Server";
        InitDB("netcode_server.db");
        ConfigureTransport(address, port);

        NetworkManager.Singleton.OnClientConnectedCallback    += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback   += OnClientDisconnected;
        NetworkManager.Singleton.StartServer();

        SpawnSyncObject();
        Log($"[Server] Started on {address}:{port}");
    }

    public void StartClient(string address, ushort port)
    {
        Role = "Client";
        InitDB("netcode_client.db");
        ConfigureTransport(address, port);

        NetworkManager.Singleton.OnClientConnectedCallback  += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.StartClient();

        Log($"[Client] Connecting to {address}:{port}...");
    }

    public void Stop()
    {
        if (NetworkManager.Singleton == null) return;
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
            NetworkManager.Singleton.Shutdown();
        else if (NetworkManager.Singleton.IsClient)
            NetworkManager.Singleton.Shutdown();

        Role = "None";
        Log("Network stopped.");
    }

    /// <summary>Add a record to the local DB (for demo seeding).</summary>
    public SyncRecord AddLocalRecord(string key, string value)
    {
        if (DB == null) { Log("DB not initialized."); return null; }
        string origin = Role == "None" ? "local" : Role.ToLower();
        var r = DB.AddRecord(origin, key, value);
        Log($"Added local record: {key}={value}");
        OnDataChanged?.Invoke();
        return r;
    }

    public List<SyncRecord> GetAllRecords() => DB?.GetAll() ?? new List<SyncRecord>();

    public void ClearLocalDB()
    {
        DB?.DeleteAll();
        Log("Local DB cleared.");
        OnDataChanged?.Invoke();
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void InitDB(string fileName)
    {
        DB?.Close();
        DB = new NetcodeDB(fileName);
        Log($"DB opened: {DB.DbPath}");
    }

    private void ConfigureTransport(string address, ushort port)
    {
        var utp = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (utp == null) utp = NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();
        utp.SetConnectionData(address, port);
    }

    private void SpawnSyncObject()
    {
        if (syncPrefab == null) { Log("ERROR: syncPrefab not assigned!"); return; }
        var go = Instantiate(syncPrefab);
        go.GetComponent<NetworkObject>().Spawn();
        _syncBehaviour = go.GetComponent<SyncNetworkBehaviour>();
        _syncBehaviour.DB            = DB;
        _syncBehaviour.OnLogMessage  = Log;
        _syncBehaviour.OnDataChanged = () => OnDataChanged?.Invoke();
    }

    private void OnClientConnected(ulong clientId)
    {
        Log($"Client connected: {clientId}");

        // If we are the server/host and the sync object exists,
        // inject DB reference so it can respond to RPCs
        if (_syncBehaviour != null)
            _syncBehaviour.DB = DB;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Log($"Client disconnected: {clientId}");
    }

    private void Log(string msg)
    {
        Debug.Log($"[SyncManager] {msg}");
        OnLog?.Invoke(msg);
    }
}
