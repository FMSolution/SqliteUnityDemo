using SQLite;

/// <summary>
/// Data model mapped to the "Person" SQLite table.
/// </summary>
[System.Serializable]
public class Person
{
    [PrimaryKey, AutoIncrement]
    public int    Id    { get; set; }
    public string Name  { get; set; }
    public int    Age   { get; set; }
    public string Email { get; set; }

    public override string ToString() =>
        $"Person(Id={Id}, Name={Name}, Age={Age}, Email={Email})";
}
