using System;
using SQLite;

/// <summary>
/// A single record that is synced between server and client SQLite databases.
/// Both server and client use this same table schema.
/// </summary>
[Serializable]
public class SyncRecord
{
    /// <summary>Auto-incremented local primary key (not synced as-is).</summary>
    [PrimaryKey, AutoIncrement]
    public int    LocalId    { get; set; }

    /// <summary>Globally unique ID assigned by the originating node.</summary>
    [Indexed]
    public string Guid       { get; set; }

    /// <summary>"server" or "client_&lt;clientId&gt;"</summary>
    public string Origin     { get; set; }

    public string Key        { get; set; }
    public string Value      { get; set; }
    public string Timestamp  { get; set; }

    public override string ToString() =>
        $"[{Origin}] {Key} = {Value}  ({Timestamp})";
}
