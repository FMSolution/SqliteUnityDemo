using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drives the SQLite demo UI: Insert / Update / Delete / Refresh operations.
/// </summary>
public class SQLiteDemoUI : MonoBehaviour
{
    [Header("Input Fields")]
    public TMP_InputField inputId;
    public TMP_InputField inputName;
    public TMP_InputField inputAge;
    public TMP_InputField inputEmail;

    [Header("Buttons")]
    public Button btnInsert;
    public Button btnUpdate;
    public Button btnDelete;
    public Button btnClear;
    public Button btnRefresh;

    [Header("Status")]
    public TMP_Text txtStatus;
    public TMP_Text txtDbPath;

    [Header("List")]
    public Transform listContainer;
    public GameObject rowPrefab;

    // Currently selected person id (-1 = none)
    private int selectedId = -1;

    private void Start()
    {
        DatabaseManager.Instance.Initialize();
        txtDbPath.text = $"DB: {DatabaseManager.Instance.DbPath}";

        btnInsert.onClick.AddListener(OnInsert);
        btnUpdate.onClick.AddListener(OnUpdate);
        btnDelete.onClick.AddListener(OnDelete);
        btnClear.onClick.AddListener(OnClearAll);
        btnRefresh.onClick.AddListener(RefreshList);

        RefreshList();
    }

    private void OnDestroy()
    {
        DatabaseManager.Instance.Close();
    }

    // ── CRUD handlers ─────────────────────────────────────────────────────────

    private void OnInsert()
    {
        if (!ValidateNameAgeEmail(out string name, out int age, out string email)) return;

        var p = DatabaseManager.Instance.Insert(name, age, email);
        SetStatus($"✔ Inserted: {p.Name} (ID {p.Id})", Color.green);
        ClearInputs();
        RefreshList();
    }

    private void OnUpdate()
    {
        if (selectedId < 0) { SetStatus("✘ Select a row first.", Color.red); return; }
        if (!ValidateNameAgeEmail(out string name, out int age, out string email)) return;

        bool ok = DatabaseManager.Instance.Update(selectedId, name, age, email);
        SetStatus(ok ? $"✔ Updated ID {selectedId}." : $"✘ ID {selectedId} not found.", ok ? Color.green : Color.red);
        ClearInputs();
        RefreshList();
    }

    private void OnDelete()
    {
        if (selectedId < 0) { SetStatus("✘ Select a row first.", Color.red); return; }

        bool ok = DatabaseManager.Instance.Delete(selectedId);
        SetStatus(ok ? $"✔ Deleted ID {selectedId}." : $"✘ ID {selectedId} not found.", ok ? Color.green : Color.red);
        ClearInputs();
        RefreshList();
    }

    private void OnClearAll()
    {
        DatabaseManager.Instance.DeleteAll();
        SetStatus("✔ All records deleted.", Color.yellow);
        ClearInputs();
        RefreshList();
    }

    // ── List ──────────────────────────────────────────────────────────────────

    public void RefreshList()
    {
        // Destroy existing rows
        foreach (Transform child in listContainer)
            Destroy(child.gameObject);

        List<Person> people = DatabaseManager.Instance.GetAll();

        foreach (Person p in people)
        {
            GameObject row = Instantiate(rowPrefab, listContainer);
            PersonRowUI rowUI = row.GetComponent<PersonRowUI>();
            rowUI.Populate(p, OnRowSelected);
        }

        SetStatus($"ℹ {people.Count} record(s) in database.", Color.white);
    }

    // ── Row selection ─────────────────────────────────────────────────────────

    private void OnRowSelected(Person p)
    {
        selectedId       = p.Id;
        inputId.text     = p.Id.ToString();
        inputName.text   = p.Name;
        inputAge.text    = p.Age.ToString();
        inputEmail.text  = p.Email;
        SetStatus($"ℹ Selected: {p.Name} (ID {p.Id})", Color.cyan);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private bool ValidateNameAgeEmail(out string name, out int age, out string email)
    {
        name  = inputName.text.Trim();
        email = inputEmail.text.Trim();
        age   = 0;

        if (string.IsNullOrEmpty(name))
        { SetStatus("✘ Name is required.", Color.red); return false; }

        if (!int.TryParse(inputAge.text.Trim(), out age) || age < 0)
        { SetStatus("✘ Age must be a non-negative integer.", Color.red); return false; }

        return true;
    }

    private void ClearInputs()
    {
        selectedId      = -1;
        inputId.text    = "";
        inputName.text  = "";
        inputAge.text   = "";
        inputEmail.text = "";
    }

    private void SetStatus(string msg, Color color)
    {
        txtStatus.text  = msg;
        txtStatus.color = color;
    }
}
