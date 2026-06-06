using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using UnityEngine.EventSystems;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public static class BuildSQLiteDemo
{
    // ── Palette ───────────────────────────────────────────────────────────────
    static Color C(string h) { ColorUtility.TryParseHtmlString("#" + h, out var c); return c; }
    static readonly Color COL_BG       = C("12111A");
    static readonly Color COL_PANEL    = C("1C1B2E");
    static readonly Color COL_HEADER   = C("252445");
    static readonly Color COL_FIELD    = C("0E0D1C");
    static readonly Color COL_ACCENT   = C("7C6AF7");
    static readonly Color COL_GREEN    = C("3DDC84");
    static readonly Color COL_BLUE     = C("4A9EFF");
    static readonly Color COL_RED      = C("FF5C5C");
    static readonly Color COL_GREY     = C("6B7280");
    static readonly Color COL_PURPLE   = C("A855F7");
    static readonly Color COL_TEXT     = C("F0EEFF");
    static readonly Color COL_MUTED    = C("8B8BA0");
    static readonly Color COL_ROW_A    = C("1E1D30");
    static readonly Color COL_ROW_B    = C("252440");

    [MenuItem("Tools/Build SQLite Demo Scene")]
    public static void Build()
    {
        // Import TMP essentials if needed
        var fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (fontAsset == null)
        {
            TMP_PackageResourceImporter.ImportResources(true, false, false);
            AssetDatabase.Refresh();
            fontAsset = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        }

        // EventSystem
        if (Object.FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
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

        // ── Root background ───────────────────────────────────────────────────
        var bgGO = Img(canvasGO, "BG", COL_BG);
        Fill(bgGO);

        // ── Main container ────────────────────────────────────────────────────
        var mainGO = Img(canvasGO, "Main", COL_PANEL);
        Fill(mainGO);
        Inset(mainGO, 24, 24, 24, 24);

        // ── Top header bar ────────────────────────────────────────────────────
        var topBar = Img(mainGO, "TopBar", COL_HEADER);
        AnchorTopStretch(topBar, 56);
        var titleLbl = TMP(topBar, "Title", "  🗄  SQLite Database Demo", fontAsset, 18, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.MidlineLeft);
        Fill(titleLbl); Inset(titleLbl, 12, 0, 0, 0);

        // ── Bottom status bar ─────────────────────────────────────────────────
        var botBar = Img(mainGO, "BotBar", COL_HEADER);
        AnchorBottomStretch(botBar, 28);
        var dbPathLbl = TMP(botBar, "DbPath", "DB: ...", fontAsset, 10, FontStyles.Normal, COL_MUTED, TextAlignmentOptions.MidlineLeft);
        Fill(dbPathLbl); Inset(dbPathLbl, 10, 0, 0, 0);

        // ── Left form panel ───────────────────────────────────────────────────
        var leftGO = Img(mainGO, "LeftPanel", COL_HEADER);
        var leftRT = leftGO.GetComponent<RectTransform>();
        leftRT.anchorMin = new Vector2(0, 0);
        leftRT.anchorMax = new Vector2(0, 1);
        leftRT.offsetMin = new Vector2(0, 30);
        leftRT.offsetMax = new Vector2(300, -60);

        // Form header
        var fhBar = Img(leftGO, "FormHeader", COL_ACCENT);
        AnchorTopStretch(fhBar, 36);
        var fhLbl = TMP(fhBar, "Lbl", "  RECORD EDITOR", fontAsset, 12, FontStyles.Bold, COL_TEXT, TextAlignmentOptions.MidlineLeft);
        Fill(fhLbl);

        // Input fields
        float fy = -44f;
        var inputId    = MakeField(leftGO, "InputId",    "ID",    "auto-assigned", fontAsset, ref fy, readOnly: true);
        var inputName  = MakeField(leftGO, "InputName",  "NAME",  "enter name",    fontAsset, ref fy);
        var inputAge   = MakeField(leftGO, "InputAge",   "AGE",   "enter age",     fontAsset, ref fy, TMP_InputField.ContentType.IntegerNumber);
        var inputEmail = MakeField(leftGO, "InputEmail", "EMAIL", "enter email",   fontAsset, ref fy);

        // Buttons
        fy -= 10f;
        var btnInsert  = MakeBtn(leftGO, "BtnInsert",  "INSERT",    COL_GREEN,  fontAsset, ref fy, 0f, 1f);
        var btnUpdate  = MakeBtn(leftGO, "BtnUpdate",  "UPDATE",    COL_BLUE,   fontAsset, ref fy, 0f, 1f);
        var btnDelete  = MakeBtn(leftGO, "BtnDelete",  "DELETE",    COL_RED,    fontAsset, ref fy, 0f, 1f);
        fy -= 4f;
        var btnClear   = MakeBtn(leftGO, "BtnClear",   "CLEAR ALL", COL_GREY,   fontAsset, ref fy, 0f,   0.47f, advance: false);
        var btnRefresh = MakeBtn(leftGO, "BtnRefresh", "REFRESH",   COL_PURPLE, fontAsset, ref fy, 0.53f, 1f,   advance: true);

        // Status label
        var statusBar = Img(leftGO, "StatusBar", C("0A0918"));
        var statusRT = statusBar.GetComponent<RectTransform>();
        statusRT.anchorMin = new Vector2(0, 0);
        statusRT.anchorMax = new Vector2(1, 0);
        statusRT.offsetMin = new Vector2(0, 0);
        statusRT.offsetMax = new Vector2(0, 32);
        var statusLbl = TMP(statusBar, "Lbl", "Ready.", fontAsset, 11, FontStyles.Normal, COL_TEXT, TextAlignmentOptions.MidlineLeft);
        Fill(statusLbl); Inset(statusLbl, 10, 0, 0, 0);

        // ── Right list panel ──────────────────────────────────────────────────
        var rightGO = Img(mainGO, "RightPanel", C("16152A"));
        var rightRT = rightGO.GetComponent<RectTransform>();
        rightRT.anchorMin = new Vector2(0, 0);
        rightRT.anchorMax = new Vector2(1, 1);
        rightRT.offsetMin = new Vector2(308, 30);
        rightRT.offsetMax = new Vector2(0, -60);

        // Column header bar
        var colBar = Img(rightGO, "ColBar", COL_HEADER);
        AnchorTopStretch(colBar, 34);
        MakeColHeaders(colBar, fontAsset);

        // ScrollRect
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(rightGO.transform, false);
        var scrollRT = scrollGO.AddComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(0, 0);
        scrollRT.offsetMax = new Vector2(0, -36);
        var sr = scrollGO.AddComponent<ScrollRect>();
        sr.horizontal = false;

        // Viewport
        var vpGO = Img(scrollGO, "Viewport", Color.clear);
        var vpRT = vpGO.GetComponent<RectTransform>();
        vpRT.anchorMin = Vector2.zero;
        vpRT.anchorMax = Vector2.one;
        vpRT.offsetMin = Vector2.zero;
        vpRT.offsetMax = new Vector2(-16, 0);
        vpGO.AddComponent<Mask>().showMaskGraphic = false;
        sr.viewport = vpRT;

        // Content
        var contentGO = new GameObject("Content");
        contentGO.transform.SetParent(vpGO.transform, false);
        var contentRT = contentGO.AddComponent<RectTransform>();
        contentRT.anchorMin        = new Vector2(0, 1);
        contentRT.anchorMax        = new Vector2(1, 1);
        contentRT.pivot            = new Vector2(0.5f, 1f);
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
        sr.content = contentRT;

        // Scrollbar
        var sbGO = new GameObject("Scrollbar");
        sbGO.transform.SetParent(scrollGO.transform, false);
        var sbRT = sbGO.AddComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(1, 0);
        sbRT.anchorMax = new Vector2(1, 1);
        sbRT.pivot     = new Vector2(1, 0.5f);
        sbRT.offsetMin = new Vector2(-16, 0);
        sbRT.offsetMax = new Vector2(0, 0);
        sbGO.AddComponent<Image>().color = C("0A0918");
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
        var handleImg = handleGO.AddComponent<Image>();
        handleImg.color = COL_ACCENT;
        sb.handleRect = handleRT;
        sb.targetGraphic = handleImg;
        sr.verticalScrollbar = sb;
        sr.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        // ── Row Prefab ────────────────────────────────────────────────────────
        var rowPrefab = BuildRowPrefab(fontAsset);

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
        demoUI.txtStatus     = statusLbl.GetComponent<TMP_Text>();
        demoUI.txtDbPath     = dbPathLbl.GetComponent<TMP_Text>();
        demoUI.listContainer = contentRT;
        demoUI.rowPrefab     = rowPrefab;

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("[SQLiteDemo] Scene built. Press Play to test.");
        Selection.activeGameObject = canvasGO;
    }

    // ── Row Prefab ────────────────────────────────────────────────────────────

    static GameObject BuildRowPrefab(TMP_FontAsset font)
    {
        var row = new GameObject("PersonRow");
        var rowRT = row.AddComponent<RectTransform>();
        rowRT.sizeDelta = new Vector2(0, 40);
        row.AddComponent<Image>().color = COL_ROW_A;

        // Columns: ID 8% | Name 28% | Age 10% | Email 34% | Btn 20%
        float[] xs = { 0f, 0.08f, 0.36f, 0.46f, 0.80f };
        float[] xe = { 0.08f, 0.36f, 0.46f, 0.80f, 1.00f };

        var txtId    = RowCell(row, "TxtId",    font, xs[0], xe[0]);
        var txtName  = RowCell(row, "TxtName",  font, xs[1], xe[1]);
        var txtAge   = RowCell(row, "TxtAge",   font, xs[2], xe[2]);
        var txtEmail = RowCell(row, "TxtEmail", font, xs[3], xe[3]);

        // Select button
        var btnGO = Img(row, "BtnSelect", COL_ACCENT);
        var btnRT = btnGO.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(xs[4], 0);
        btnRT.anchorMax = new Vector2(xe[4], 1);
        btnRT.offsetMin = new Vector2(6, 5);
        btnRT.offsetMax = new Vector2(-6, -5);
        var btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnGO.GetComponent<Image>();
        var bc = btn.colors;
        bc.highlightedColor = C("9D8FFF");
        bc.pressedColor = C("5A4FCC");
        btn.colors = bc;
        var btnLbl = TMP(btnGO, "Lbl", "SELECT", font, 11, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        Fill(btnLbl);

        var rowUI = row.AddComponent<PersonRowUI>();
        rowUI.txtId    = txtId;
        rowUI.txtName  = txtName;
        rowUI.txtAge   = txtAge;
        rowUI.txtEmail = txtEmail;
        rowUI.btnSelect = btn;

        Directory.CreateDirectory("Assets/Prefabs");
        var prefab = PrefabUtility.SaveAsPrefabAsset(row, "Assets/Prefabs/PersonRow.prefab");
        Object.DestroyImmediate(row);
        return prefab;
    }

    static TMP_Text RowCell(GameObject parent, string name, TMP_FontAsset font, float xMin, float xMax)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 0);
        rt.anchorMax = new Vector2(xMax, 1);
        rt.offsetMin = new Vector2(8, 0);
        rt.offsetMax = new Vector2(-2, 0);
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.fontSize = 12;
        t.color = COL_TEXT;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.overflowMode = TextOverflowModes.Ellipsis;
        return t;
    }

    static void MakeColHeaders(GameObject parent, TMP_FontAsset font)
    {
        string[] labels = { "ID", "NAME", "AGE", "EMAIL", "ACTION" };
        float[]  xs     = { 0f,    0.08f, 0.36f, 0.46f,  0.80f };
        float[]  xe     = { 0.08f, 0.36f, 0.46f, 0.80f,  1.00f };
        for (int i = 0; i < labels.Length; i++)
        {
            var go = new GameObject("Col" + i);
            go.transform.SetParent(parent.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(xs[i], 0);
            rt.anchorMax = new Vector2(xe[i], 1);
            rt.offsetMin = new Vector2(8, 0);
            rt.offsetMax = new Vector2(0, 0);
            var t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = labels[i];
            t.fontSize = 11;
            t.fontStyle = FontStyles.Bold;
            t.color = COL_ACCENT;
            t.alignment = TextAlignmentOptions.MidlineLeft;
        }
    }

    // ── Input Field ───────────────────────────────────────────────────────────

    static TMP_InputField MakeField(GameObject parent, string id, string labelText,
        string placeholder, TMP_FontAsset font, ref float y,
        TMP_InputField.ContentType ct = TMP_InputField.ContentType.Standard,
        bool readOnly = false)
    {
        const float LH = 16f, FH = 36f, GAP = 8f;

        // Label
        var lblGO = new GameObject(id + "_Lbl");
        lblGO.transform.SetParent(parent.transform, false);
        var lblRT = lblGO.AddComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(0, 1);
        lblRT.anchorMax = new Vector2(1, 1);
        lblRT.anchoredPosition = new Vector2(0, y - LH * 0.5f);
        lblRT.sizeDelta = new Vector2(-20, LH);
        var lblT = lblGO.AddComponent<TextMeshProUGUI>();
        if (font != null) lblT.font = font;
        lblT.text = labelText;
        lblT.fontSize = 10;
        lblT.fontStyle = FontStyles.Bold;
        lblT.color = COL_MUTED;
        lblT.alignment = TextAlignmentOptions.MidlineLeft;
        y -= LH;

        // Field background
        var fGO = new GameObject(id);
        fGO.transform.SetParent(parent.transform, false);
        var fRT = fGO.AddComponent<RectTransform>();
        fRT.anchorMin = new Vector2(0, 1);
        fRT.anchorMax = new Vector2(1, 1);
        fRT.anchoredPosition = new Vector2(0, y - FH * 0.5f);
        fRT.sizeDelta = new Vector2(-20, FH);
        fGO.AddComponent<Image>().color = readOnly ? C("0A0918") : COL_FIELD;

        var field = fGO.AddComponent<TMP_InputField>();
        field.contentType = ct;
        field.readOnly = readOnly;

        // Text area
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
        if (font != null) phT.font = font;
        phT.text = placeholder;
        phT.fontSize = 13;
        phT.color = C("4A4860");
        phT.fontStyle = FontStyles.Italic;
        phT.alignment = TextAlignmentOptions.MidlineLeft;

        // Input text
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(taGO.transform, false);
        var txtRT = txtGO.AddComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero; txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
        var txtT = txtGO.AddComponent<TextMeshProUGUI>();
        if (font != null) txtT.font = font;
        txtT.fontSize = 13;
        txtT.color = COL_TEXT;
        txtT.alignment = TextAlignmentOptions.MidlineLeft;

        field.textViewport  = taRT;
        field.textComponent = txtT;
        field.placeholder   = phT;
        field.caretColor    = COL_ACCENT;
        field.selectionColor = new Color(COL_ACCENT.r, COL_ACCENT.g, COL_ACCENT.b, 0.3f);

        y -= FH + GAP;
        return field;
    }

    // ── Button ────────────────────────────────────────────────────────────────

    static Button MakeBtn(GameObject parent, string id, string text, Color color,
        TMP_FontAsset font, ref float y, float xMin, float xMax, bool advance = true)
    {
        const float H = 36f, GAP = 5f;

        var go = Img(parent, id, color);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, 1);
        rt.anchorMax = new Vector2(xMax, 1);
        rt.anchoredPosition = new Vector2(0, y - H * 0.5f);
        rt.sizeDelta = new Vector2(-20, H);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var bc = btn.colors;
        bc.highlightedColor = Lighten(color, 0.15f);
        bc.pressedColor = Darken(color, 0.2f);
        btn.colors = bc;

        var lbl = TMP(go, "Lbl", text, font, 12, FontStyles.Bold, Color.white, TextAlignmentOptions.Center);
        Fill(lbl);

        if (advance) y -= H + GAP;
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

    static GameObject TMP(GameObject parent, string name, string text,
        TMP_FontAsset font, float size, FontStyles style, Color color, TextAlignmentOptions align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.AddComponent<RectTransform>();
        var t = go.AddComponent<TextMeshProUGUI>();
        if (font != null) t.font = font;
        t.text = text; t.fontSize = size;
        t.fontStyle = style; t.color = color; t.alignment = align;
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
        rt.offsetMin = new Vector2(l, b);
        rt.offsetMax = new Vector2(-r, -t);
    }

    static void AnchorTopStretch(GameObject go, float height)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1); rt.anchorMax = new Vector2(1, 1);
        rt.offsetMin = new Vector2(0, -height); rt.offsetMax = new Vector2(0, 0);
    }

    static void AnchorBottomStretch(GameObject go, float height)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 0); rt.anchorMax = new Vector2(1, 0);
        rt.offsetMin = new Vector2(0, 0); rt.offsetMax = new Vector2(0, height);
    }

    static Color Lighten(Color c, float a) =>
        new Color(Mathf.Min(c.r + a, 1), Mathf.Min(c.g + a, 1), Mathf.Min(c.b + a, 1), c.a);
    static Color Darken(Color c, float a) =>
        new Color(Mathf.Max(c.r - a, 0), Mathf.Max(c.g - a, 0), Mathf.Max(c.b - a, 0), c.a);
}
