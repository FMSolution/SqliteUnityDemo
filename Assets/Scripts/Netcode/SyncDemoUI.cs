using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI controller for the Netcode SQLite Sync demo.
/// </summary>
public class SyncDemoUI : MonoBehaviour
{
    [Header("Connection")]
    public TMP_InputField inputAddress;
    public TMP_InputField inputPort;
    public Button         btnHost;
    public Button         btnServer;
    public Button         btnClient;
    public Button         btnStop;
    public TMP_Text       txtRole;
    public TMP_Text       txtDbPath;

    [Header("Data Entry")]
    public TMP_InputField inputKey;
    public TMP_InputField inputValue;
    public Button         btnAddRecord;
    public Button         btnClearDB;

    [Header("Records Table")]
    public Transform      tableContent;
    public GameObject     rowPrefab;

    [Header("Log")]
    public ScrollRect     logScrollRect;
    public Transform      logContent;
    public GameObject     logRowPrefab;

    private SyncManager _mgr;
    private const int MAX_LOG = 100;
    private readonly List<GameObject> _logRows = new List<GameObject>();

    private void Start()
    {
        _mgr = SyncManager.Instance;
        _mgr.OnLog         += AppendLog;
        _mgr.OnDataChanged += RefreshTable;

        btnHost.onClick.AddListener(OnHost);
        btnServer.onClick.AddListener(OnServer);
        btnClient.onClick.AddListener(OnClient);
        btnStop.onClick.AddListener(OnStop);
        btnAddRecord.onClick.AddListener(OnAddRecord);
        btnClearDB.onClick.AddListener(OnClearDB);

        SetStoppedState();
        RefreshTable();
    }

    private void OnDestroy()
    {
        if (_mgr == null) return;
        _mgr.OnLog         -= AppendLog;
        _mgr.OnDataChanged -= RefreshTable;
    }

    // ── Button handlers ───────────────────────────────────────────────────────

    private void OnHost()
    {
        if (!GetAddressPort(out string addr, out ushort port)) return;
        _mgr.StartHost(addr, port);
        SetRunningState("HOST");
    }

    private void OnServer()
    {
        if (!GetAddressPort(out string addr, out ushort port)) return;
        _mgr.StartServer(addr, port);
        SetRunningState("SERVER");
    }

    private void OnClient()
    {
        if (!GetAddressPort(out string addr, out ushort port)) return;
        _mgr.StartClient(addr, port);
        SetRunningState("CLIENT");
    }

    private void OnStop()
    {
        _mgr.Stop();
        SetStoppedState();
    }

    private void OnAddRecord()
    {
        string key = inputKey.text.Trim();
        string val = inputValue.text.Trim();
        if (string.IsNullOrEmpty(key)) { AppendLog("Key is required."); return; }
        _mgr.AddLocalRecord(key, val);
        inputKey.text   = "";
        inputValue.text = "";
    }

    private void OnClearDB()
    {
        _mgr.ClearLocalDB();
        RefreshTable();
    }

    // ── Table ─────────────────────────────────────────────────────────────────

    private void RefreshTable()
    {
        foreach (Transform child in tableContent)
            Destroy(child.gameObject);

        var records = _mgr.GetAllRecords();
        txtDbPath.text = _mgr.DB != null ? $"DB: {_mgr.DB.DbPath}" : "DB: not open";

        foreach (var r in records)
        {
            var row = Instantiate(rowPrefab, tableContent);
            var cells = row.GetComponentsInChildren<TMP_Text>();
            if (cells.Length >= 4)
            {
                cells[0].text = r.Origin;
                cells[1].text = r.Key;
                cells[2].text = r.Value;
                cells[3].text = r.Timestamp;
            }
        }
    }

    // ── Log ───────────────────────────────────────────────────────────────────

    private void AppendLog(string msg)
    {
        while (_logRows.Count >= MAX_LOG)
        {
            Destroy(_logRows[0]);
            _logRows.RemoveAt(0);
        }

        var row = Instantiate(logRowPrefab, logContent);
        var t   = row.GetComponentInChildren<TMP_Text>();
        if (t != null) t.text = $"[{System.DateTime.Now:HH:mm:ss}] {msg}";
        _logRows.Add(row);

        Canvas.ForceUpdateCanvases();
        logScrollRect.verticalNormalizedPosition = 0f;
    }

    // ── State helpers ─────────────────────────────────────────────────────────

    private void SetRunningState(string role)
    {
        txtRole.text = $"Role: {role}";
        btnHost.interactable   = false;
        btnServer.interactable = false;
        btnClient.interactable = false;
        btnStop.interactable   = true;
        RefreshTable();
    }

    private void SetStoppedState()
    {
        txtRole.text = "Role: None";
        btnHost.interactable   = true;
        btnServer.interactable = true;
        btnClient.interactable = true;
        btnStop.interactable   = false;
        txtDbPath.text         = "DB: not open";
    }

    private bool GetAddressPort(out string address, out ushort port)
    {
        address = inputAddress.text.Trim();
        port    = 7777;
        if (string.IsNullOrEmpty(address)) address = "127.0.0.1";
        if (!ushort.TryParse(inputPort.text.Trim(), out port)) port = 7777;
        return true;
    }
}
