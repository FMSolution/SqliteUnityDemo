using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;

public static class SQLiteDemoSceneBuilder
{
    static readonly Color BG_DARK      = Hex("1A1A2E");
    static readonly Color BG_PANEL     = Hex("16213E");
    static readonly Color BG_HEADER    = Hex("0F3460");
    static readonly Color ACCENT       = Hex("E94560");
    static readonly Color BTN_INSERT   = Hex("27AE60");
    static readonly Color BTN_UPDATE   = Hex("2980B9");
    static readonly Color BTN_DELETE   = Hex("C0392B");
    static readonly Color BTN_CLEAR    = Hex("7F8C8D");
    static readonly Color BTN_REFRESH  = Hex("8E44AD");
    static readonly Color TEXT_PRIMARY = Hex("EAEAEA");
    static readonly Color TEXT_MUTED   = Hex("95A5A6");
    static readonly Color INPUT_BG     = Hex("0D1B2A");

    [MenuItem("Tools/Build SQLite Demo Scene")]
    public static void Build()
    {
        // Ensure TMP Essential Resources exist
        if (Resources.Load("Fonts & Materials/LiberationSans SDF") == null)
            TMP_PackageResourceImporter.ImportResources(true, false, false);

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // ── Canvas ────────────────────────────────────────────────────────────
        var canvasGO = new GameObject("SQLiteDemo Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Background ────────────────────────────────────────────────────────
        var bg = Panel(canvasGO, "Background", BG_DARK);
        FullStretch(bg);

        // ── Main Panel ────────────────────────────────────────────────────────
        var main = Panel(canvasGO, "MainPanel", BG_PANEL);
        FullStretch(main);
        Pad(main, 20, 20, 20, 20);

        // ── Header ────────────────────────────────────────────────────────────
        var header = Panel(main, "Header", BG_HEADER);
        SetAnchors(header, 0, 1, 1, 1);
        SetOffsets(header, 0, -60, 0, 0);
        Label(header, "Title", "  SQLite Database Demo", 20, FontStyles.Bold, TEXT_PRIMARY, TextAlignmentOptions.MidlineLeft);

        // ── DB Path ───────────────────────────────────────────────────────────
        var dbPathGO = Panel(main, "DbPathBar", Hex("0A1020"));
        SetAnchors(dbPathGO, 0, 0, 1, 0);
        SetOffsets(dbPathGO, 0, 0, 0, 24);
        var dbPathLabel = Label(dbPathGO, "DbPath", "DB Path: initializing...", 10, FontStyles.Normal, TEXT_MUTED, TextAlignmentOptions.MidlineLeft);
        FullStretch(dbPathLabel);
        Pad(dbPathLabel, 8, 0, 0, 0);

        // ── Left Panel ────────────────────────────────────────────────────────
        var left = Panel(main, "LeftPanel", Hex("080F1E"));
        SetAnchors(left, 0, 0, 0, 1);
        SetOffsets(left, 0, 26, 320, -64);

        // Form title
        var formTitle = Panel(left, "FormTitleBar", BG_HEADER);
        SetAnchors(formTitle, 0, 1, 1, 1);
        SetOffsets(formTitle, 0, -40, 0, 0);
        Label(formTitle, "Lbl", "  RECORD EDITOR", 12, FontStyles.Bold, ACCENT, TextAlignmentOptions.MidlineLeft);

        // ── Input Fields ──────────────────────────────────────────────────────
        float top = -48f;
        var inputId    = InputField(left, "InputId",    "ID",    "Auto-assigned", ref top, readOnly: true);
        var inputName  = InputField(left, "InputName",  "NAME",  "Enter name",    ref top);
        var inputAge   = InputField(left, "InputAge",   "AGE",   "Enter age",     ref top, TMP_InputField.ContentType.IntegerNumber);
        var inputEmail = InputField(left, "InputEmail", "EMAIL", "Enter email",   ref top);

        // ── Buttons ───────────────────────────────────────────────────────────
        top -= 8f;
        var btnInsert  = Btn(left, "BtnInsert",  "INSERT",    BTN_INSERT,  ref top, 0f,   1f);
        var btnUpdate  = Btn(left, "BtnUpdate",  "UPDATE",    BTN_UPDATE,  ref top, 0f,   1f);
        var btnDelete  = Btn(left, "BtnDelete",  "DELETE",    BTN_DELETE,  ref top, 0f,   1f);
        top -= 4f;
        var btnClear   = Btn(left, "BtnClear",   "CLEAR ALL", BTN_CLEAR,   ref top, 0f,   0.48f, advance: false);
        var btnRefresh = Btn(left, "BtnRefresh", "REFRESH",   BTN_REFRESH, ref top, 0.52f, 1f,   advance: true);

        // Status
        var statusGO = Panel(left, "StatusBar", Hex("0A1020"));
        SetAnchors(statusGO, 0, 0, 1, 0);
        SetOffsets(statusGO, 0, 0, 0, 30);
        var statusLabel = Label(statusGO, "Lbl", "Ready.", 11, FontStyles.Normal, TEXT_PRIMARY, TextAlignmentOptions.MidlineLeft);
        FullStretch(statusLabel);
        Pad(statusLabel, 8, 0, 0, 0);

        // ── Right Panel ───────────────────────────────────────────────────────
        var right = Panel(main, "RightPanel", Hex("080F1E"));
        SetAnchors(right, 0, 0, 1, 1);
        SetOffsets(right, 328, 26, 0, -64);

        // Column headers
        var colBar = Panel(right, "ColBar", BG_HEADER);
        SetAnchors(colBar, 0, 1, 1, 1);
        SetOffsets(colBar, 0, -32, 0, 0);
        MakeColHeaders(colBar);

        // ── ScrollView ────────────────────────────────────────────────────────
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(right.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        SetAnchors(scrollGO, 0, 0, 1, 1);
        SetOffsets(scrollGO, 0, 0, 0, -34);
        var scrollRect = scrollGO.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;

        // Viewport
        var vpGO = Panel(scrollGO, "Viewport", Color.clear);
        SetAnchors(vpGO, 0, 0, 1, 1);
        SetOffsets(vpGO, 0, 0, -14, 0);
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        scrollRect.viewport = vpGO.GetComponent<RectTransform>();

        // Content
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin        = new Vector2(0, 1);
        contentRT.anchorMax        = new Vector2(1, 1);
        contentRT.pivot            = new Vector2(0.5f, 1);
        contentRT.anchoredPosition = Vector2.zero;
        contentRT.sizeDelta        = Vector2.zero;
        var vlg = contentGO.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 2;
        vlg.padding = new RectOffset(2, 2, 2, 2);
        vlg.childForceExpandWidth  = true;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth  = true;
        vlg.childControlHeight = true;
        var csf = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = contentRT;

        // Scrollbar
        var sbGO = new GameObject("Scrollbar");
        sbGO.transform.SetParent(scrollGO.transform, false);
        SetAnchors(sbGO, 1, 0, 1, 1);
        SetOffsets(sbGO, -14, 0, 0, 0);
        sbGO.AddComponent<Image>().color = Hex("0A1020");
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
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = ACCENT;
        sb.handleRect = handleRT;
        sb.targetGraphic = handleImg;
        scrollRect.verticalScrollbar = sb;
        scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // ── Row Prefab ────────────────────────────────────────────────────────
        var rowPrefab = BuildRowPrefab();

        // ── Wire SQLiteDemoUI ─────────────────────────────────────────────────
        var demoUI = canvasGO.AddComponent<SQLiteDemoUI>();
        demoUI.inputId       = inputId;
        demoUI.inputName     = inputName;
        demoUI.inputAge      = inputAge;
        demoUI.inputEmail    = inputEmail;
        demoUI.btnInsert     = btnInsert;
        demoUI.btnUpdate     = btnUpdate;
        demoUI.btnDelete     = btnDelete;
        demoUI.btnClear      = btnClear;
        demoUI.btnRefresh    = btnRefresh;
        demoUI.txtStatus     = statusLabel.GetComponent<TMP_Text>();
        demoUI.txtDbPath     = dbPathLabel.GetComponent<TMP_Text>();
        demoUI.listContainer = contentRT;
        demoUI.rowPrefab     = rowPrefab;

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene());

        Debug.Log("[SQLiteDemo] Scene built successfully. Press Play to test.");
        Selection.activeGameObject = canvasGO;
    }

    // ── Row Prefab ────────────────────────────────────────────────────────────

    static GameObject BuildRowPrefab()
    {
        var row = new GameObject("PersonRow");
        var rowRT = row.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 38);
        row.AddComponent<Image>().color = Hex("1A2540");

        // Columns: ID 8% | Name 27% | Age 10% | Email 35% | Btn 20%
        float[] xs = { 0f, 0.08f, 0.35f, 0.45f, 0.80f, 1f };

        var txtId    = RowCell(row, "TxtId",    xs[0], xs[1]);
        var txtName  = RowCell(row, "TxtName",  xs[1], xs[2]);
        var txtAge   = RowCell(row, "TxtAge",   xs[2], xs[3]);
        var txtEmail = RowCell(row, "TxtEmail", xs[3], xs[4]);

        // Select button
        var btnGO = new GameObject("BtnSelect");
        btnGO.transform.SetParent(row.transform, false);
        var btnRT = btnGO.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(xs[4], 0);
        btnRT.anchorMax = new Vector2(xs[5], 1);
        btnRT.offsetMin = new Vector2(4, 4);
        btnRT.offsetMax = new Vector2(-4, -4);
        var btnImg = btnGO.AddComponent<Image>();
        btnImg.color = ACCENT;
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        var cb = btn.colors;
        cb.highlightedColor = Lighten(ACCENT, 0.2f);
        cb.pressedColor = Darken(ACCENT, 0.2f);
        btn.colors = cb;

        var btnLbl = new GameObject("Lbl");
        btnLbl.transform.SetParent(btnGO.transform, false);
        var blRT = btnLbl.AddComponent<RectTransform>();
        blRT.anchorMin = Vector2.zero; blRT.anchorMax = Vector2.one;
        blRT.offsetMin = Vector2.zero; blRT.offsetMax = Vector2.zero;
        var blT = btnLbl.AddComponent<TextMeshProUGUI>();
        blT.text = "SELECT"; blT.fontSize = 11;
        blT.fontStyle = FontStyles.Bold;
        blT.color = Color.white;
        blT.alignment = TextAlignmentOptions.Center;

        var rowUI = row.AddComponent<PersonRowUI>();
        rowUI.txtId    = txtId;
        rowUI.txtName  = txtName;
        rowUI.txtAge   = txtAge;
        rowUI.txtEmail = txtEmail;
        rowUI.btnSelect = btn;

        System.IO.Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(row, "Assets/Prefabs/PersonRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    static TMP_Text RowCell(GameObject parent, string name, float xMin, float xMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 0);
        rt.anchorMax = new Vector2(xMax, 1);
        rt.offsetMin = new Vector2(6, 0);
        rt.offsetMax = new Vector2(-2, 0);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.fontSize = 12;
        t.color = TEXT_PRIMARY;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }

    static void MakeColHeaders(GameObject parent)
    {
        string[] names  = { "ID",   "NAME", "AGE",  "EMAIL", "" };
        float[]  xs     = { 0f,    0.08f,  0.35f,  0.45f,  0.80f };
        float[]  xe     = { 0.08f, 0.35f,  0.45f,  0.80f,  1.00f };
        for (int i = 0; i < names.Length; i++)
        {
            var go = new GameObject("Col_" + names[i]);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xs[i], 0);
            rt.anchorMax = new Vector2(xe[i], 1);
            rt.offsetMin = new Vector2(8, 0);
            rt.offsetMax = new Vector2(0, 0);
            var t = go.AddComponent<TextMeshProUGUI>();
            t.text = names[i]; t.fontSize = 11;
            t.fontStyle = FontStyles.Bold;
            t.color = ACCENT;
            t.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    // ── Input Field ───────────────────────────────────────────────────────────

    static TMP_InputField InputField(GameObject parent, string name, string labelText,
        string placeholder, ref float top,
        TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard,
        bool readOnly = false)
    {
        const float LH = 18f, FH = 38f, GAP = 6f;

        // Label
        var lblGO = new GameObject(name + "_Label");
        lblGO.transform.SetParent(parent.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0, 1);
        lblRT.anchorMax = new Vector2(1, 1);
        lblRT.anchoredPosition = new Vector2(0, top - LH * 0.5f);
        lblRT.sizeDelta = new Vector2(-20, LH);
        var lblT = lblGO.AddComponent<TextMeshProUGUI>();
        lblT.text = labelText; lblT.fontSize = 10;
        lblT.color = TEXT_MUTED;
        lblT.fontStyle = FontStyles.Bold;
        lblT.alignment = TextAlignmentOptions.MidlineLeft;
        top -= LH;

        // Field BG
        var fGO = new GameObject(name);
        fGO.transform.SetParent(parent.transform, false);
        var fRT = fGO.AddComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 1);
        fRT.anchorMax = new Vector2(1, 1);
        fRT.anchoredPosition = new Vector2(0, top - FH * 0.5f);
        fRT.sizeDelta = new Vector2(-20, FH);
        fGO.AddComponent<Image>().color = readOnly ? Hex("0C1828") : INPUT_BG;

        var field = fGO.AddComponent<TMP_InputField>();
        field.contentType = contentType;
        field.readOnly = readOnly;

        // Text Area
        var taGO = new GameObject("TextArea");
        taGO.transform.SetParent(fGO.transform, false);
        var taRT = taGO.AddComponent<RectTransform>();
        taRT.anchorMin = Vector2.zero; taRT.anchorMax = Vector2.one;
        taRT.offsetMin = new Vector2(8, 2); taRT.offsetMax = new Vector2(-8, -2);
        taGO.AddComponent<RectMask2D>();

        // Placeholder
        var phGO = new GameObject("Placeholder");
        phGO.transform.SetParent(taGO.transform, false);
        var phRT = phGO.AddComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero; phRT.anchorMax = Vector2.one;
        phRT.offsetMin = Vector2.zero; phRT.offsetMax = Vector2.zero;
        var phT = phGO.AddComponent<TextMeshProUGUI>();
        phT.text = placeholder; phT.fontSize = 13;
        phT.color = new Color(0.45f, 0.45f, 0.55f, 1f);
        phT.fontStyle = FontStyles.Italic;
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        // Text
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(taGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        var txtT = txtGO.AddComponent<TextMeshProUGUI>();
        txtT.fontSize = 13; txtT.color = TEXT_PRIMARY;
        txtT.alignment = TextAlignmentOptions.MidlineLeft;

        field.textViewport  = taRT;
        field.textComponent = txtT;
        field.placeholder   = phT;
        field.caretColor    = ACCENT;
        field.selectionColor = new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.35f);

        top -= FH + GAP;
        return field;
    }

    // ── Button ────────────────────────────────────────────────────────────────

    static Button Btn(GameObject parent, string name, string text, Color color,
        ref float top, float xMin, float xMax, bool advance = true)
    {
        const float H = 36f, GAP = 5f, PAD = 10f;

        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 1);
        rt.anchorMax = new Vector2(xMax, 1);
        rt.anchoredPosition = new Vector2(0, top - H * 0.5f);
        rt.sizeDelta = new Vector2(-(PAD * 2f / (xMax - xMin)), H);

        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.highlightedColor = Lighten(color, 0.15f);
        cb.pressedColor = Darken(color, 0.2f);
        btn.colors = cb;

        var lblGO = new GameObject("Lbl");
        lblGO.transform.SetParent(go.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = Vector2.zero; lblRT.anchorMax = Vector2.one;
        lblRT.offsetMin = Vector2.zero; lblRT.offsetMax = Vector2.zero;
        var lblT = lblGO.AddComponent<TextMeshProUGUI>();
        lblT.text = text; lblT.fontSize = 12;
        lblT.fontStyle = FontStyles.Bold;
        lblT.color = Color.white;
        lblT.alignment = TextAlignmentOptions.Center;

        if (advance) top -= H + GAP;
        return btn;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static GameObject Panel(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        go.AddComponent<Image>().color = color;
        return go;
    }

    static GameObject Label(GameObject parent, string name, string text,
        float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text; t.fontSize = size;
        t.fontStyle = style; t.color = color; t.alignment = align;
        return go;
    }

    static void FullStretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    static void Pad(GameObject go, float left, float right, float bottom, float top)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    static void SetAnchors(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
    }

    // offsetMin = (left, bottom), offsetMax = (right from right edge, top from top edge) — negative = inward
    static void SetOffsets(GameObject go, float left, float bottom, float right, float top)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }

    static Color Lighten(Color c, float a) =>
        new Color(Mathf.Min(c.r+a,1), Mathf.Min(c.g+a,1), Mathf.Min(c.b+a,1), c.a);

    static Color Darken(Color c, float a) =>
        new Color(Mathf.Max(c.r-a,0), Mathf.Max(c.g-a,0), Mathf.Max(c.b-a,0), c.a);
}
