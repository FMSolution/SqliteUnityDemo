using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Represents a single row in the person list.
/// </summary>
public class PersonRowUI : MonoBehaviour
{
    public TMP_Text txtId;
    public TMP_Text txtName;
    public TMP_Text txtAge;
    public TMP_Text txtEmail;
    public Button   btnSelect;

    private Person _person;
    private Action<Person> _onSelected;

    public void Populate(Person person, Action<Person> onSelected)
    {
        _person     = person;
        _onSelected = onSelected;

        txtId.text    = person.Id.ToString();
        txtName.text  = person.Name;
        txtAge.text   = person.Age.ToString();
        txtEmail.text = person.Email;

        btnSelect.onClick.AddListener(() => _onSelected?.Invoke(_person));
    }
}
