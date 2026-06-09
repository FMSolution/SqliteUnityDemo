using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public static class BuildNetcodeSyncScene
{
    static Color C(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }

    static readonly Color BG       = C("0B0B14");
    static readonly Color PANEL    = C("111120");
    static readonly Color HEADER   = C("1A1A30");
    static readonly Color ACCENT   = C("00C8FF");
    static readonly Color GREEN    = C("00E676");
    static readonly Color ORANGE   = C("FF9800");
    static readonly Color BLUE     = C("2979FF");
    static readonly Color RED      = C("FF5252");
    static readonly Color GREY     = C("546E7A");
    static readonly Color TEXT     = C("E8F4FD");
    static readonly Color MUTED    = C("78909C");
    static readonly Color FIELD_BG = C("07070F");
    static readonly Color ROW_A    = C("131325");
    static readonly Color ROW_B    = C("0F0F1E");

    [MenuItem("Tools/Build Netcode Sync Scene")]
    public static void Build()
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        // ── EventSystem ───────────────────────────────────────────────────────
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ── NetworkManager ────────────────────────────────────────────────────
        var nmGO = new GameObject("NetworkManager");
        var nm   = nmGO.AddComponent<NetworkManager>();
        var utp  = nmGO.AddComponent<UnityTransport>();
        nm.NetworkConfig = new NetworkConfig();
        nm.NetworkConfig.NetworkTransport = utp;

        // ── SyncManager ───────────────────────────────────────────────────────
        var smGO = new GameObject("SyncManager");
        var sm   = smGO.AddComponent<SyncManager>();

        // ── Sync Prefab ───────────────────────────────────────────────────────
        var syncPrefab = BuildSyncPrefab(nm);
        sm.syncPrefab  = syncPrefab;

        // ── Canvas ────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("NetcodeSync Canvas");
        var canvas   = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight  = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        var bg = Img(canvasGO, "BG", BG); Fill(bg);
        var main = Img(canvasGO, "Main", PANEL); Fill(main); Inset(main, 16, 16, 16, 16);

        // ── Header ────────────────────────────────────────────────────────────
        var topBar = Img(main, "TopBar", HEADER); TopStretch(topBar, 48);
        Lbl(topBar, "Title", "  ⚡  Unity Netcode  ×  SQLite  —  Bidirectional DB Sync",
            font, 15, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        // ── Left column: Connection + Data Entry ──────────────────────────────
        var left = Img(main, "Left", HEADER);
        var lRT  = left.GetComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0, 0); lRT.anchorMax = new Vector2(0, 1);
        lRT.offsetMin = new Vector2(0, 0); lRT.offsetMax = new Vector2(290, -52);

        // Section: Network
        var netHdr = Img(left, "NetHdr", C("0A0A20")); TopStretch(netHdr, 28);
        Lbl(netHdr, "L", "  NETWORK", font, 10, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        float y = -36f;
        var inputAddr = MakeField(left, "InputAddr", "ADDRESS", "127.0.0.1", font, ref y);
        var inputPort = MakeField(left, "InputPort", "PORT",    "7777",      font, ref y);

        y -= 4f;
        var btnHost   = MakeBtn(left, "BtnHost",   "START HOST",   GREEN,  font, ref y, 0f, 1f);
        var btnServer = MakeBtn(left, "BtnServer", "START SERVER", ORANGE, font, ref y, 0f, 1f);
        var btnClient = MakeBtn(left, "BtnClient", "START CLIENT", BLUE,   font, ref y, 0f, 1f);
        var btnStop   = MakeBtn(left, "BtnStop",   "STOP",         RED,    font, ref y, 0f, 1f);

        y -= 4f;
        var roleBar = Img(left, "RoleBar", C("07070F"));
        var rbRT = roleBar.GetComponent<RectTransform>();
        rbRT.anchorMin = new Vector2(0,1); rbRT.anchorMax = new Vector2(1,1);
        rbRT.offsetMin = new Vector2(0, y-24); rbRT.offsetMax = new Vector2(0, y);
        y -= 24f;
        var txtRole = Lbl(roleBar, "TxtRole", "Role: None", font, 11, FontStyles.Bold, MUTED, TextAlignmentOptions.MidlineLeft);
        Fill(txtRole); Inset(txtRole, 8, 0, 0, 0);

        // Section: Data Entry
        y -= 10f;
        var dataHdr = Img(left, "DataHdr", C("0A0A20"));
        var dhRT = dataHdr.GetComponent<RectTransform>();
        dhRT.anchorMin = new Vector2(0,1); dhRT.anchorMax = new Vector2(1,1);
        dhRT.offsetMin = new Vector2(0, y-28); dhRT.offsetMax = new Vector2(0, y);
        y -= 28f;
        Lbl(dataHdr, "L", "  LOCAL DATA", font, 10, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        var inputKey = MakeField(left, "InputKey",   "KEY",   "sensor_temp", font, ref y);
        var inputVal = MakeField(left, "InputValue", "VALUE", "23.5",        font, ref y);

        y -= 4f;
        var btnAdd   = MakeBtn(left, "BtnAdd",   "ADD RECORD", GREEN, font, ref y, 0f,   1f);
        var btnClear = MakeBtn(left, "BtnClear", "CLEAR DB",   RED,   font, ref y, 0f,   1f);

        // DB path at bottom of left panel
        var dbBar = Img(left, "DbBar", C("07070F"));
        var dbRT  = dbBar.GetComponent<RectTransform>();
        dbRT.anchorMin = new Vector2(0,0); dbRT.anchorMax = new Vector2(1,0);
        dbRT.offsetMin = new Vector2(0,0); dbRT.offsetMax = new Vector2(0,28);
        var txtDbPath = Lbl(dbBar, "TxtDb", "DB: not open", font, 9, FontStyles.Normal, MUTED, TextAlignmentOptions.MidlineLeft);
        Fill(txtDbPath); Inset(txtDbPath, 6, 0, 0, 0);

        // ── Right column: Table + Log ─────────────────────────────────────────
        var right = Img(main, "Right", C("0D0D1C"));
        var rRT   = right.GetComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0,0); rRT.anchorMax = new Vector2(1,1);
        rRT.offsetMin = new Vector2(298, 0); rRT.offsetMax = new Vector2(0, -52);

        // Table header
        var tblHdr = Img(right, "TblHdr", HEADER); TopStretch(tblHdr, 28);
        MakeTableColHeaders(tblHdr, font);

        // Table scroll
        var (tableScroll, tableContent) = MakeScrollArea(right, font,
            new Vector2(0,0), new Vector2(1,1),
            new Vector2(0, 160), new Vector2(0, -30));

        // Log header
        var logHdrBar = Img(right, "LogHdr", C("0A0A20"));
        var lhRT = logHdrBar.GetComponent<RectTransform>();
        lhRT.anchorMin = new Vector2(0,0); lhRT.anchorMax = new Vector2(1,0);
        lhRT.offsetMin = new Vector2(0, 130); lhRT.offsetMax = new Vector2(0, 158);
        Lbl(logHdrBar, "L", "  EVENT LOG", font, 10, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        // Log scroll
        var (logScroll, logContent) = MakeScrollArea(right, font,
            new Vector2(0,0), new Vector2(1,0),
            new Vector2(0, 0), new Vector2(0, 128));

        // ── Row prefabs ───────────────────────────────────────────────────────
        var tableRowPrefab = BuildTableRowPrefab(font);
        var logRowPrefab   = BuildLogRowPrefab(font);

        // ── Wire SyncDemoUI ───────────────────────────────────────────────────
        var demoUI = canvasGO.AddComponent<SyncDemoUI>();
        demoUI.inputAddress  = inputAddr;
        demoUI.inputPort     = inputPort;
        demoUI.btnHost       = btnHost;
        demoUI.btnServer     = btnServer;
        demoUI.btnClient     = btnClient;
        demoUI.btnStop       = btnStop;
        demoUI.txtRole       = txtRole.GetComponent<TMP_Text>();
        demoUI.txtDbPath     = txtDbPath.GetComponent<TMP_Text>();
        demoUI.inputKey      = inputKey;
        demoUI.inputValue    = inputVal;
        demoUI.btnAddRecord  = btnAdd;
        demoUI.btnClearDB    = btnClear;
        demoUI.tableContent  = tableContent;
        demoUI.rowPrefab     = tableRowPrefab;
        demoUI.logScrollRect = logScroll;
        demoUI.logContent    = logContent;
        demoUI.logRowPrefab  = logRowPrefab;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[NetcodeSync] Scene built. Press Play to test.");
        Selection.activeGameObject = canvasGO;
    }

    // ── Prefabs ───────────────────────────────────────────────────────────────

    static GameObject BuildSyncPrefab(NetworkManager nm)
    {
        var go = new GameObject("SyncObject");
        var netObj = go.AddComponent<NetworkObject>();
        go.AddComponent<SyncNetworkBehaviour>();

        Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/SyncObject.prefab");
        Object.DestroyImmediate(go);

        // Register with NetworkManager prefab list
        nm.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = prefab });

        return prefab;
    }

    static GameObject BuildTableRowPrefab(TMP_FontAsset font)
    {
        var row = new GameObject("TableRow");
        var rt  = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 30);
        row.AddComponent<Image>().color = ROW_A;

        // Columns: Origin 20% | Key 25% | Value 30% | Timestamp 25%
        float[] xs = { 0f, 0.20f, 0.45f, 0.75f };
        float[] xe = { 0.20f, 0.45f, 0.75f, 1.00f };
        string[] names = { "Origin", "Key", "Value", "Timestamp" };
        for (int i = 0; i < 4; i++)
        {
            var cell = new GameObject(names[i]);
            cell.transform.SetParent(row.transform, false);
            var cRT = cell.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(xs[i], 0); cRT.anchorMax = new Vector2(xe[i], 1);
            cRT.offsetMin = new Vector2(6, 0); cRT.offsetMax = new Vector2(-2, 0);
            var t = cell.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = 11; t.color = TEXT;
            t.alignment = TextAlignmentOptions.MidlineLeft;
            t.overflowMode = TextOverflowModes.Ellipsis;
        }

        Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(row, "Assets/Prefabs/SyncTableRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    static GameObject BuildLogRowPrefab(TMP_FontAsset font)
    {
        var row = new GameObject("LogRow");
        var rt  = row.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 22);

        var cell = new GameObject("Text");
        cell.transform.SetParent(row.transform, false);
        var cRT = cell.AddComponent<RectTransform>();
        cRT.anchorMin = Vector2.zero; cRT.anchorMax = Vector2.one;
        cRT.offsetMin = new Vector2(6, 0); cRT.offsetMax = new Vector2(-4, 0);
        var t = cell.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 10; t.color = MUTED;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.overflowMode = TextOverflowModes.Ellipsis;

        Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(row, "Assets/Prefabs/SyncLogRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    static void MakeTableColHeaders(GameObject parent, TMP_FontAsset font)
    {
        string[] labels = { "ORIGIN", "KEY", "VALUE", "TIMESTAMP" };
        float[]  xs     = { 0f,    0.20f, 0.45f, 0.75f };
        float[]  xe     = { 0.20f, 0.45f, 0.75f, 1.00f };
        for (int i = 0; i < labels.Length; i++)
        {
            var go = new GameObject("Col" + i);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xs[i], 0); rt.anchorMax = new Vector2(xe[i], 1);
            rt.offsetMin = new Vector2(8, 0); rt.offsetMax = new Vector2(0, 0);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = labels[i]; t.fontSize = 10;
            t.fontStyle = FontStyles.Bold; t.color = ACCENT;
            t.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    // ── Scroll area factory ───────────────────────────────────────────────────

    static (ScrollRect scroll, Transform content) MakeScrollArea(
        GameObject parent, TMP_FontAsset font,
        Vector2 anchorMin, Vector2 anchorMax,
        Vector2 offsetMin, Vector2 offsetMax)
    {
        var scrollGO = new GameObject("Scroll");
        scrollGO.transform.SetParent(parent.transform, false);
        var sRT = scrollGO.AddComponent<RectTransform>();
        sRT.anchorMin = anchorMin; sRT.anchorMax = anchorMax;
        sRT.offsetMin = offsetMin; sRT.offsetMax = offsetMax;
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false;

        var vpGO = Img(scrollGO, "Viewport", Color.clear);
        var vpRT = vpGO.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero; vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero; vpRT.offsetMax = new Vector2(-14, 0);
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var cRT = contentGO.AddComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0,1); cRT.anchorMax = new Vector2(1,1);
        cRT.pivot = new Vector2(0.5f,1f);
        cRT.anchoredPosition = Vector2.zero; cRT.sizeDelta = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 1; vlg.padding = new RectOffset(1,1,1,1);
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = cRT;

        // Scrollbar
        var sbGO = new GameObject("Scrollbar");
        sbGO.transform.SetParent(scrollGO.transform, false);
        var sbRT = sbGO.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1,0); sbRT.anchorMax = new Vector2(1,1);
        sbRT.pivot = new Vector2(1,0.5f);
        sbRT.offsetMin = new Vector2(-14,0); sbRT.offsetMax = Vector2.zero;
        sbGO.AddComponent<Image>().color = C("07070F");
        var sb = sbGO.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        var slideGO = new GameObject("SlidingArea");
        slideGO.transform.SetParent(sbGO.transform, false);
        var slideRT = slideGO.AddComponent<RectTransform>();
        slideRT.anchorMin = Vector2.zero; slideRT.anchorMax = Vector2.one;
        slideRT.offsetMin = new Vector2(2,2); slideRT.offsetMax = new Vector2(-2,-2);
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(slideGO.transform, false);
        var handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.anchorMin = Vector2.zero; handleRT.anchorMax = Vector2.one;
        handleRT.offsetMin = Vector2.zero; handleRT.offsetMax = Vector2.zero;
        var handleImg = handleGO.AddComponent<Image>(); handleImg.color = ACCENT;
        sb.handleRect = handleRT; sb.targetGraphic = handleImg;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        return (sr, cRT);
    }

    // ── Input field ───────────────────────────────────────────────────────────

    static TMP_InputField MakeField(GameObject parent, string id, string label,
        string placeholder, TMP_FontAsset font, ref float y)
    {
        const float LH = 15f, FH = 34f, GAP = 6f;

        var lblGO = new GameObject(id + "_Lbl");
        lblGO.transform.SetParent(parent.transform, false);
        var lRT = lblGO.AddComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0,1); lRT.anchorMax = new Vector2(1,1);
        lRT.anchoredPosition = new Vector2(0, y - LH*0.5f);
        lRT.sizeDelta = new Vector2(-16, LH);
        var lT = lblGO.AddComponent<TextMeshProUGUI>();
        if (font != null) lT.font = font;
        lT.text = label; lT.fontSize = 9; lT.fontStyle = FontStyles.Bold;
        lT.color = MUTED; lT.alignment = TextAlignmentOptions.MidlineLeft;
        y -= LH;

        var fGO = new GameObject(id);
        fGO.transform.SetParent(parent.transform, false);
        var fRT = fGO.AddComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0,1); fRT.anchorMax = new Vector2(1,1);
        fRT.anchoredPosition = new Vector2(0, y - FH*0.5f);
        fRT.sizeDelta = new Vector2(-16, FH);
        fGO.AddComponent<Image>().color = FIELD_BG;

        var field = fGO.AddComponent<TMP_InputField>();
        var taGO = new GameObject("TextArea");
        taGO.transform.SetParent(fGO.transform, false);
        var taRT = taGO.AddComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(6,2); taRT.offsetMax = new Vector2(-6,-2);
        taGO.AddComponent<RectMask2D>();

        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(taGO.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
        var phT = phGO.AddComponent<TextMeshProUGUI>();
        if (font != null) phT.font = font;
        phT.text = placeholder; phT.fontSize = 12;
        phT.color = C("2A2A40"); phT.fontStyle = FontStyles.Italic;
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(taGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        var txtT = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) txtT.font = font;
        txtT.fontSize = 12; txtT.color = TEXT;
        txtT.alignment = TextAlignmentOptions.MidlineLeft;

        field.textViewport = taRT; field.textComponent = txtT;
        field.placeholder = phT; field.caretColor = ACCENT;
        y -= FH + GAP;
        return field;
    }

    // ── Button ────────────────────────────────────────────────────────────────

    static Button MakeBtn(GameObject parent, string id, string text, Color color,
        TMP_FontAsset font, ref float y, float xMin, float xMax)
    {
        const float H = 32f, GAP = 4f;
        var go = Img(parent, id, color);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin,1); rt.anchorMax = new Vector2(xMax,1);
        rt.anchoredPosition = new Vector2(0, y - H*0.5f);
        rt.sizeDelta = new Vector2(-16, H);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var bc = btn.colors;
        bc.highlightedColor = Lighten(color, 0.15f);
        bc.pressedColor = Darken(color, 0.2f);
        btn.colors = bc;
        Lbl(go, "Lbl", text, font, 11, FontStyles.Bold, Color.black, TextAlignmentOptions.Center);
        y -= H + GAP;
        return btn;
    }

    // ── Primitives ────────────────────────────────────────────────────────────

    static GameObject Img(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    static GameObject Lbl(GameObject parent, string name, string text,
        TMP_FontAsset font, float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text; t.fontSize = size; t.fontStyle = style;
        t.color = color; t.alignment = align;
        return go;
    }

    static void Fill(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Inset(GameObject go, float l, float r, float b, float t)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(l, b); rt.offsetMax = new Vector2(-r, -t);
    }

    static void TopStretch(GameObject go, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0,1); rt.anchorMax = new Vector2(1,1);
        rt.offsetMin = new Vector2(0,-h); rt.offsetMax = Vector2.zero;
    }

    static Color Lighten(Color c, float a) =>
        new Color(Mathf.Min(c.r+a,1), Mathf.Min(c.g+a,1), Mathf.Min(c.b+a,1), c.a);
    static Color Darken(Color c, float a) =>
        new Color(Mathf.Max(c.r-a,0), Mathf.Max(c.g-a,0), Mathf.Max(c.b-a,0), c.a);
}
