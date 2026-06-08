using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BuildSocketIODemoScene
{
    static Color C(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }

    static readonly Color BG         = C("0D0D1A");
    static readonly Color PANEL      = C("13132A");
    static readonly Color HEADER     = C("1A1A3A");
    static readonly Color ACCENT     = C("00D4FF");
    static readonly Color GREEN      = C("00E676");
    static readonly Color RED        = C("FF5252");
    static readonly Color GREY       = C("546E7A");
    static readonly Color TEXT       = C("E8F4FD");
    static readonly Color MUTED      = C("78909C");
    static readonly Color FIELD_BG   = C("080818");
    static readonly Color CHAT_BG    = C("0A0A1E");

    [MenuItem("Tools/Build SocketIO Demo Scene")]
    public static void Build()
    {
        var font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (font == null)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();
            font = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // SocketIOClientManager
        if (Object.FindObjectOfType<SocketIOClientManager>() == null)
        {
            var mgr = new GameObject("SocketIOClientManager");
            mgr.AddComponent<SocketIOClientManager>();
        }

        // ── Canvas ────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("SocketIODemo Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Background
        var bg = Img(canvasGO, "BG", BG); Fill(bg);

        // Main container
        var main = Img(canvasGO, "Main", PANEL); Fill(main); Inset(main, 20, 20, 20, 20);

        // ── Top header ────────────────────────────────────────────────────────
        var topBar = Img(main, "TopBar", HEADER);
        TopStretch(topBar, 52);
        Lbl(topBar, "Title", "  ⚡  Socket.IO Demo  —  Real-time Chat", font, 17, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        // ── Left panel: Connection + Controls ─────────────────────────────────
        var left = Img(main, "LeftPanel", HEADER);
        var leftRT = left.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0, 0);
        leftRT.anchorMax = new Vector2(0, 1);
        leftRT.offsetMin = new Vector2(0, 0);
        leftRT.offsetMax = new Vector2(300, -56);

        // Section: Connection
        var connHeader = Img(left, "ConnHeader", C("0A0A28"));
        TopStretch(connHeader, 32);
        Lbl(connHeader, "Lbl", "  CONNECTION", font, 11, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        float y = -40f;

        // Server URL
        var inputUrl = MakeField(left, "InputUrl", "SERVER URL", "ws://localhost:3000", font, ref y);

        // Username
        var inputUser = MakeField(left, "InputUser", "USERNAME", "Player1", font, ref y);

        // Connect / Disconnect buttons
        y -= 4f;
        var btnConnect    = MakeBtn(left, "BtnConnect",    "CONNECT",    GREEN, font, ref y, 0f,   1f);
        var btnDisconnect = MakeBtn(left, "BtnDisconnect", "DISCONNECT", RED,   font, ref y, 0f,   1f);

        // Status
        y -= 6f;
        var statusRow = Img(left, "StatusRow", C("080818"));
        var srRT = statusRow.GetComponent<RectTransform>();
        srRT.anchorMin = new Vector2(0, 1); srRT.anchorMax = new Vector2(1, 1);
        srRT.offsetMin = new Vector2(0, y - 28); srRT.offsetMax = new Vector2(0, y);
        y -= 28f;

        var statusDot = Img(statusRow, "Dot", GREY);
        var dotRT = statusDot.GetComponent<RectTransform>();
        dotRT.anchorMin = new Vector2(0, 0.5f); dotRT.anchorMax = new Vector2(0, 0.5f);
        dotRT.anchoredPosition = new Vector2(14, 0); dotRT.sizeDelta = new Vector2(10, 10);

        var txtStatus = Lbl(statusRow, "TxtStatus", "Disconnected", font, 12, FontStyles.Normal, GREY, TextAlignmentOptions.MidlineLeft);
        var tsRT = txtStatus.GetComponent<RectTransform>();
        tsRT.anchorMin = new Vector2(0, 0); tsRT.anchorMax = new Vector2(1, 1);
        tsRT.offsetMin = new Vector2(28, 0); tsRT.offsetMax = new Vector2(0, 0);

        // Online count
        y -= 4f;
        var onlineRow = Img(left, "OnlineRow", C("080818"));
        var orRT = onlineRow.GetComponent<RectTransform>();
        orRT.anchorMin = new Vector2(0, 1); orRT.anchorMax = new Vector2(1, 1);
        orRT.offsetMin = new Vector2(0, y - 26); orRT.offsetMax = new Vector2(0, y);
        y -= 26f;
        var txtOnline = Lbl(onlineRow, "TxtOnline", "Online: 0", font, 11, FontStyles.Normal, MUTED, TextAlignmentOptions.MidlineLeft);
        Fill(txtOnline); Inset(txtOnline, 10, 0, 0, 0);

        // ── Right panel: Chat ─────────────────────────────────────────────────
        var right = Img(main, "RightPanel", CHAT_BG);
        var rightRT = right.GetComponent<RectTransform>();
        rightRT.anchorMin = new Vector2(0, 0);
        rightRT.anchorMax = new Vector2(1, 1);
        rightRT.offsetMin = new Vector2(308, 0);
        rightRT.offsetMax = new Vector2(0, -56);

        // Chat header
        var chatHeader = Img(right, "ChatHeader", HEADER);
        TopStretch(chatHeader, 32);
        Lbl(chatHeader, "Lbl", "  CHAT LOG", font, 11, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        // Message input bar at bottom
        var inputBar = Img(right, "InputBar", HEADER);
        BottomStretch(inputBar, 48);

        var inputMsg = MakeInlineField(inputBar, "InputMsg", "Type a message and press Enter...", font);

        var btnSend = Img(inputBar, "BtnSend", ACCENT);
        var bsRT = btnSend.GetComponent<RectTransform>();
        bsRT.anchorMin = new Vector2(1, 0); bsRT.anchorMax = new Vector2(1, 1);
        bsRT.offsetMin = new Vector2(-100, 6); bsRT.offsetMax = new Vector2(-6, -6);
        var btnSendBtn = btnSend.AddComponent<Button>();
        btnSendBtn.targetGraphic = btnSend.GetComponent<Image>();
        var bc = btnSendBtn.colors;
        bc.highlightedColor = C("33DDFF"); bc.pressedColor = C("0099BB");
        btnSendBtn.colors = bc;
        Lbl(btnSend, "Lbl", "SEND", font, 12, FontStyles.Bold, Color.black, TextAlignmentOptions.Center);

        // ScrollRect for chat
        var scrollGO = new GameObject("ChatScroll");
        scrollGO.transform.SetParent(right.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0); scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(0, 50); scrollRT.offsetMax = new Vector2(0, -34);
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
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);
        contentRT.anchoredPosition = Vector2.zero; contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 1; vlg.padding = new RectOffset(2, 2, 2, 2);
        vlg.childForceExpandWidth = true; vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true; vlg.childControlHeight = true;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sr.content = contentRT;

        // Scrollbar
        var sbGO = new GameObject("Scrollbar");
        sbGO.transform.SetParent(scrollGO.transform, false);
        var sbRT = sbGO.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0); sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot = new Vector2(1, 0.5f);
        sbRT.offsetMin = new Vector2(-14, 0); sbRT.offsetMax = Vector2.zero;
        sbGO.AddComponent<Image>().color = C("080818");
        var sb = sbGO.AddComponent<Scrollbar>();
        sb.direction = Scrollbar.Direction.BottomToTop;
        var slideGO = new GameObject("SlidingArea");
        slideGO.transform.SetParent(sbGO.transform, false);
        var slideRT = slideGO.AddComponent<RectTransform>();
        slideRT.anchorMin = Vector2.zero; slideRT.anchorMax = Vector2.one;
        slideRT.offsetMin = new Vector2(2, 2); slideRT.offsetMax = new Vector2(-2, -2);
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(slideGO.transform, false);
        var handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.anchorMin = Vector2.zero; handleRT.anchorMax = Vector2.one;
        handleRT.offsetMin = Vector2.zero; handleRT.offsetMax = Vector2.zero;
        var handleImg = handleGO.AddComponent<Image>(); handleImg.color = ACCENT;
        sb.handleRect = handleRT; sb.targetGraphic = handleImg;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // ── Chat Row Prefab ───────────────────────────────────────────────────
        var rowPrefab = BuildChatRowPrefab(font);

        // ── Wire SocketIODemoUI ───────────────────────────────────────────────
        var demoUI = canvasGO.AddComponent<SocketIODemoUI>();
        demoUI.inputServerUrl      = inputUrl;
        demoUI.inputUsername       = inputUser;
        demoUI.btnConnect          = btnConnect;
        demoUI.btnDisconnect       = btnDisconnect;
        demoUI.txtConnectionStatus = txtStatus.GetComponent<TMP_Text>();
        demoUI.txtOnlineCount      = txtOnline.GetComponent<TMP_Text>();
        demoUI.inputMessage        = inputMsg;
        demoUI.btnSend             = btnSendBtn;
        demoUI.chatScrollRect      = sr;
        demoUI.chatContent         = contentRT;
        demoUI.chatRowPrefab       = rowPrefab;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SocketIODemo] Scene built. Start the Node server then press Play.");
        Selection.activeGameObject = canvasGO;
    }

    // ── Chat Row Prefab ───────────────────────────────────────────────────────

    static GameObject BuildChatRowPrefab(TMP_FontAsset font)
    {
        var row = new GameObject("ChatRow");
        var rowRT = row.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 44);
        var rowImg = row.AddComponent<Image>();
        rowImg.color = C("0A0A1E");

        var rowUI = row.AddComponent<ChatRowUI>();
        rowUI.rowBackground = rowImg;

        // User label (left, fixed width)
        var userGO = new GameObject("TxtUser");
        userGO.transform.SetParent(row.transform, false);
        var uRT = userGO.AddComponent<RectTransform>();
        uRT.anchorMin = new Vector2(0, 0); uRT.anchorMax = new Vector2(0, 1);
        uRT.offsetMin = new Vector2(8, 0); uRT.offsetMax = new Vector2(140, 0);
        var uT = userGO.AddComponent<TextMeshProUGUI>();
        if (font != null) uT.font = font;
        uT.fontSize = 11; uT.fontStyle = FontStyles.Bold;
        uT.color = C("4FC3F7"); uT.alignment = TextAlignmentOptions.MidlineLeft;
        uT.overflowMode = TextOverflowModes.Ellipsis;
        rowUI.txtUser = uT;

        // Message text (fills remaining space)
        var msgGO = new GameObject("TxtMessage");
        msgGO.transform.SetParent(row.transform, false);
        var mRT = msgGO.AddComponent<RectTransform>();
        mRT.anchorMin = new Vector2(0, 0); mRT.anchorMax = new Vector2(1, 1);
        mRT.offsetMin = new Vector2(148, 0); mRT.offsetMax = new Vector2(-80, 0);
        var mT = msgGO.AddComponent<TextMeshProUGUI>();
        if (font != null) mT.font = font;
        mT.fontSize = 12; mT.color = C("E8F4FD");
        mT.alignment = TextAlignmentOptions.MidlineLeft;
        mT.overflowMode = TextOverflowModes.Ellipsis;
        rowUI.txtMessage = mT;

        // Timestamp (right-aligned)
        var tsGO = new GameObject("TxtTimestamp");
        tsGO.transform.SetParent(row.transform, false);
        var tRT = tsGO.AddComponent<RectTransform>();
        tRT.anchorMin = new Vector2(1, 0); tRT.anchorMax = new Vector2(1, 1);
        tRT.offsetMin = new Vector2(-78, 0); tRT.offsetMax = new Vector2(-6, 0);
        var tT = tsGO.AddComponent<TextMeshProUGUI>();
        if (font != null) tT.font = font;
        tT.fontSize = 10; tT.color = C("546E7A");
        tT.alignment = TextAlignmentOptions.MidlineRight;
        rowUI.txtTimestamp = tT;

        Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(row, "Assets/Prefabs/ChatRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    // ── Input field helpers ───────────────────────────────────────────────────

    static TMP_InputField MakeField(GameObject parent, string id, string label,
        string placeholder, TMP_FontAsset font, ref float y)
    {
        const float LH = 16f, FH = 36f, GAP = 8f;

        var lblGO = new GameObject(id + "_Lbl");
        lblGO.transform.SetParent(parent.transform, false);
        var lRT = lblGO.AddComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0, 1); lRT.anchorMax = new Vector2(1, 1);
        lRT.anchoredPosition = new Vector2(0, y - LH * 0.5f);
        lRT.sizeDelta = new Vector2(-20, LH);
        var lT = lblGO.AddComponent<TextMeshProUGUI>();
        if (font != null) lT.font = font;
        lT.text = label; lT.fontSize = 10; lT.fontStyle = FontStyles.Bold;
        lT.color = MUTED; lT.alignment = TextAlignmentOptions.MidlineLeft;
        y -= LH;

        var fGO = new GameObject(id);
        fGO.transform.SetParent(parent.transform, false);
        var fRT = fGO.AddComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 1); fRT.anchorMax = new Vector2(1, 1);
        fRT.anchoredPosition = new Vector2(0, y - FH * 0.5f);
        fRT.sizeDelta = new Vector2(-20, FH);
        fGO.AddComponent<Image>().color = FIELD_BG;

        var field = fGO.AddComponent<TMP_InputField>();

        var taGO = new GameObject("TextArea");
        taGO.transform.SetParent(fGO.transform, false);
        var taRT = taGO.AddComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(8, 2); taRT.offsetMax = new Vector2(-8, -2);
        taGO.AddComponent<RectMask2D>();

        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(taGO.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
        var phT = phGO.AddComponent<TextMeshProUGUI>();
        if (font != null) phT.font = font;
        phT.text = placeholder; phT.fontSize = 12;
        phT.color = C("3A3A5A"); phT.fontStyle = FontStyles.Italic;
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(taGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        var txtT = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) txtT.font = font;
        txtT.fontSize = 13; txtT.color = TEXT;
        txtT.alignment = TextAlignmentOptions.MidlineLeft;

        field.textViewport = taRT; field.textComponent = txtT;
        field.placeholder = phT; field.caretColor = ACCENT;

        y -= FH + GAP;
        return field;
    }

    static TMP_InputField MakeInlineField(GameObject parent, string id, string placeholder, TMP_FontAsset font)
    {
        var fGO = new GameObject(id);
        fGO.transform.SetParent(parent.transform, false);
        var fRT = fGO.AddComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 0); fRT.anchorMax = new Vector2(1, 1);
        fRT.offsetMin = new Vector2(8, 6); fRT.offsetMax = new Vector2(-108, -6);
        fGO.AddComponent<Image>().color = FIELD_BG;

        var field = fGO.AddComponent<TMP_InputField>();

        var taGO = new GameObject("TextArea");
        taGO.transform.SetParent(fGO.transform, false);
        var taRT = taGO.AddComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(8, 2); taRT.offsetMax = new Vector2(-8, -2);
        taGO.AddComponent<RectMask2D>();

        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(taGO.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
        var phT = phGO.AddComponent<TextMeshProUGUI>();
        if (font != null) phT.font = font;
        phT.text = placeholder; phT.fontSize = 12;
        phT.color = C("3A3A5A"); phT.fontStyle = FontStyles.Italic;
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(taGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        var txtT = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) txtT.font = font;
        txtT.fontSize = 13; txtT.color = TEXT;
        txtT.alignment = TextAlignmentOptions.MidlineLeft;

        field.textViewport = taRT; field.textComponent = txtT;
        field.placeholder = phT; field.caretColor = ACCENT;
        return field;
    }

    // ── Button ────────────────────────────────────────────────────────────────

    static Button MakeBtn(GameObject parent, string id, string text, Color color,
        TMP_FontAsset font, ref float y, float xMin, float xMax)
    {
        const float H = 36f, GAP = 5f;
        var go = Img(parent, id, color);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 1); rt.anchorMax = new Vector2(xMax, 1);
        rt.anchoredPosition = new Vector2(0, y - H * 0.5f);
        rt.sizeDelta = new Vector2(-20, H);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var bc = btn.colors;
        bc.highlightedColor = Lighten(color, 0.15f);
        bc.pressedColor = Darken(color, 0.2f);
        btn.colors = bc;
        Lbl(go, "Lbl", text, font, 12, FontStyles.Bold, Color.black, TextAlignmentOptions.Center);
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
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, -h); rt.offsetMax = Vector2.zero;
    }

    static void BottomStretch(GameObject go, float h)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = Vector2.zero; rt.offsetMax = new Vector2(0, h);
    }

    static Color Lighten(Color c, float a) =>
        new Color(Mathf.Min(c.r + a, 1), Mathf.Min(c.g + a, 1), Mathf.Min(c.b + a, 1), c.a);
    static Color Darken(Color c, float a) =>
        new Color(Mathf.Max(c.r - a, 0), Mathf.Max(c.g - a, 0), Mathf.Max(c.b - a, 0), c.a);
}
