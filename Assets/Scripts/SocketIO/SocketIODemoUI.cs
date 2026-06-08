using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the Socket.IO demo UI.
/// </summary>
public class SocketIODemoUI : MonoBehaviour
{
    [Header("Connection Panel")]
    public TMP_InputField inputServerUrl;
    public TMP_InputField inputUsername;
    public Button         btnConnect;
    public Button         btnDisconnect;
    public TMP_Text       txtConnectionStatus;
    public TMP_Text       txtOnlineCount;

    [Header("Chat Panel")]
    public TMP_InputField inputMessage;
    public Button         btnSend;
    public ScrollRect     chatScrollRect;
    public Transform      chatContent;
    public GameObject     chatRowPrefab;

    // Max messages to keep in the log
    private const int MAX_MESSAGES = 200;
    private readonly List<GameObject> _rows = new List<GameObject>();

    private SocketIOClientManager _client;

    private void Start()
    {
        _client = SocketIOClientManager.Instance;

        // Subscribe to events
        _client.OnConnected     += HandleConnected;
        _client.OnDisconnected  += HandleDisconnected;
        _client.OnChatMessage   += HandleChatMessage;
        _client.OnUserJoined    += HandleUserJoined;
        _client.OnUserLeft      += HandleUserLeft;
        _client.OnOnlineCount   += HandleOnlineCount;

        // Button listeners
        btnConnect.onClick.AddListener(OnClickConnect);
        btnDisconnect.onClick.AddListener(OnClickDisconnect);
        btnSend.onClick.AddListener(OnClickSend);
        inputMessage.onSubmit.AddListener(_ => OnClickSend());

        SetDisconnectedState();
    }

    private void OnDestroy()
    {
        if (_client == null) return;
        _client.OnConnected    -= HandleConnected;
        _client.OnDisconnected -= HandleDisconnected;
        _client.OnChatMessage  -= HandleChatMessage;
        _client.OnUserJoined   -= HandleUserJoined;
        _client.OnUserLeft     -= HandleUserLeft;
        _client.OnOnlineCount  -= HandleOnlineCount;
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnClickConnect()
    {
        string url      = inputServerUrl.text.Trim();
        string username = inputUsername.text.Trim();

        if (string.IsNullOrEmpty(url))      { SetStatus("Enter a server URL.", Color.red); return; }
        if (string.IsNullOrEmpty(username)) { SetStatus("Enter a username.",   Color.red); return; }

        SetStatus("Connecting...", Color.yellow);
        btnConnect.interactable = false;
        _client.Connect(url, username);
    }

    private void OnClickDisconnect()
    {
        _client.Disconnect();
    }

    private void OnClickSend()
    {
        string msg = inputMessage.text.Trim();
        if (string.IsNullOrEmpty(msg)) return;
        if (!_client.IsConnected) { SetStatus("Not connected.", Color.red); return; }

        _client.SendMessage(msg);
        inputMessage.text = "";
        inputMessage.ActivateInputField();
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandleConnected()
    {
        SetStatus("Connected", Color.green);
        btnConnect.interactable    = false;
        btnDisconnect.interactable = true;
        btnSend.interactable       = true;
        inputMessage.interactable  = true;
        AddSystemMessage("✔ Connected to server.");
    }

    private void HandleDisconnected(string reason)
    {
        SetDisconnectedState();
        AddSystemMessage($"✘ Disconnected: {reason}");
    }

    private void HandleChatMessage(string user, string text, string timestamp)
    {
        AddChatRow(user, text, timestamp, isSystem: false);
    }

    private void HandleUserJoined(string username, int count)
    {
        txtOnlineCount.text = $"Online: {count}";
        AddSystemMessage($"→ {username} joined. ({count} online)");
    }

    private void HandleUserLeft(string username, int count)
    {
        txtOnlineCount.text = $"Online: {count}";
        AddSystemMessage($"← {username} left. ({count} online)");
    }

    private void HandleOnlineCount(int count)
    {
        txtOnlineCount.text = $"Online: {count}";
    }

    // ── UI helpers ────────────────────────────────────────────────────────────

    private void SetDisconnectedState()
    {
        SetStatus("Disconnected", Color.gray);
        btnConnect.interactable    = true;
        btnDisconnect.interactable = false;
        btnSend.interactable       = false;
        inputMessage.interactable  = false;
        txtOnlineCount.text        = "Online: 0";
    }

    private void SetStatus(string msg, Color color)
    {
        txtConnectionStatus.text  = msg;
        txtConnectionStatus.color = color;
    }

    private void AddSystemMessage(string text)
    {
        AddChatRow("SYSTEM", text, "", isSystem: true);
    }

    private void AddChatRow(string user, string text, string timestamp, bool isSystem)
    {
        // Trim old rows
        while (_rows.Count >= MAX_MESSAGES)
        {
            Destroy(_rows[0]);
            _rows.RemoveAt(0);
        }

        var row = Instantiate(chatRowPrefab, chatContent);
        var rowUI = row.GetComponent<ChatRowUI>();
        rowUI.Populate(user, text, timestamp, isSystem);
        _rows.Add(row);

        // Scroll to bottom next frame
        Canvas.ForceUpdateCanvases();
        chatScrollRect.verticalNormalizedPosition = 0f;
    }
}
