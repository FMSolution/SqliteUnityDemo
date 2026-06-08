using System;
using System.Text.Json;
using UnityEngine;

/// <summary>
/// Singleton that manages the Socket.IO connection.
/// ALL events are dispatched on the Unity main thread.
/// </summary>
public class SocketIOClientManager : MonoBehaviour
{
    public static SocketIOClientManager Instance { get; private set; }

    // ── Events — always fired on Unity main thread ────────────────────────────
    public event Action                         OnConnected;
    public event Action<string>                 OnDisconnected;
    public event Action<string, string, string> OnChatMessage;  // user, text, timestamp
    public event Action<string, int>            OnUserJoined;   // username, onlineCount
    public event Action<string, int>            OnUserLeft;     // username, onlineCount
    public event Action<int>                    OnOnlineCount;

    private SocketIOUnity _socket;
    public bool IsConnected => _socket != null && _socket.Connected;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy() => Disconnect();

    // ── Public API ────────────────────────────────────────────────────────────

    public void Connect(string serverUrl, string username)
    {
        if (_socket != null) Disconnect();

        Debug.Log($"[Socket.IO] Connecting to {serverUrl}");

        var options = new SocketIOClient.SocketIOOptions
        {
            Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
            ReconnectionAttempts = 5
        };

        _socket = new SocketIOUnity(serverUrl, options);

        // ── OnConnected fires on background thread → dispatch to main thread ──
        _socket.OnConnected += (sender, e) =>
        {
            UnityThread.executeInUpdate(() =>
            {
                Debug.Log("[Socket.IO] Connected!");
                OnConnected?.Invoke();
                // Send username after connecting
                _socket.Emit("set_username", new { username });
            });
        };

        // ── OnDisconnected fires on background thread → dispatch to main thread
        _socket.OnDisconnected += (sender, reason) =>
        {
            UnityThread.executeInUpdate(() =>
            {
                Debug.Log($"[Socket.IO] Disconnected: {reason}");
                OnDisconnected?.Invoke(reason);
            });
        };

        _socket.OnError += (sender, err) =>
        {
            UnityThread.executeInUpdate(() =>
            {
                Debug.LogError($"[Socket.IO] Error: {err}");
            });
        };

        // ── Server → client events (OnUnityThread already dispatches to main) ─

        // chat_message: { user, text, timestamp }
        _socket.OnUnityThread("chat_message", response =>
        {
            try
            {
                var el      = response.GetValue<JsonElement>();
                string user = GetString(el, "user",      "?");
                string text = GetString(el, "text",      "");
                string ts   = GetString(el, "timestamp", "");
                OnChatMessage?.Invoke(user, text, ts);
            }
            catch (Exception ex) { Debug.LogError($"[Socket.IO] chat_message parse: {ex.Message}"); }
        });

        // user_joined: { username, onlineCount }
        _socket.OnUnityThread("user_joined", response =>
        {
            try
            {
                var el    = response.GetValue<JsonElement>();
                string un = GetString(el, "username",    "?");
                int count = GetInt(el,    "onlineCount", 0);
                OnUserJoined?.Invoke(un, count);
            }
            catch (Exception ex) { Debug.LogError($"[Socket.IO] user_joined parse: {ex.Message}"); }
        });

        // user_left: { username, onlineCount }
        _socket.OnUnityThread("user_left", response =>
        {
            try
            {
                var el    = response.GetValue<JsonElement>();
                string un = GetString(el, "username",    "?");
                int count = GetInt(el,    "onlineCount", 0);
                OnUserLeft?.Invoke(un, count);
            }
            catch (Exception ex) { Debug.LogError($"[Socket.IO] user_left parse: {ex.Message}"); }
        });

        // online_count: { count }
        _socket.OnUnityThread("online_count", response =>
        {
            try
            {
                var el    = response.GetValue<JsonElement>();
                int count = GetInt(el, "count", 0);
                OnOnlineCount?.Invoke(count);
            }
            catch (Exception ex) { Debug.LogError($"[Socket.IO] online_count parse: {ex.Message}"); }
        });

        _socket.ConnectAsync();
    }

    public void Disconnect()
    {
        if (_socket == null) return;
        _socket.DisconnectAsync();
        _socket = null;
    }

    public void SendMessage(string text)
    {
        if (!IsConnected) return;
        _socket.Emit("chat_message", new { text });
    }

    // ── JsonElement helpers ───────────────────────────────────────────────────

    private static string GetString(JsonElement el, string key, string fallback)
    {
        if (el.TryGetProperty(key, out var prop) && prop.ValueKind == JsonValueKind.String)
            return prop.GetString() ?? fallback;
        return fallback;
    }

    private static int GetInt(JsonElement el, string key, int fallback)
    {
        if (el.TryGetProperty(key, out var prop) &&
            prop.ValueKind == JsonValueKind.Number &&
            prop.TryGetInt32(out int v))
            return v;
        return fallback;
    }
}
