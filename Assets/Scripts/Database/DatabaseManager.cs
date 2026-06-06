using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SQLite;

/// <summary>
/// Manages a SQLite database connection and CRUD operations for the Person table.
/// </summary>
public class DatabaseManager
{
    private static DatabaseManager _instance;
    public static DatabaseManager Instance => _instance ?? (_instance = new DatabaseManager());

    private SQLiteConnection _db;
    private const string DbFileName = "demo.db";

    public string DbPath { get; private set; }

    private DatabaseManager() { }

    /// <summary>Opens (or creates) the database and ensures the table exists.</summary>
    public void Initialize()
    {
        DbPath = Path.Combine(Application.persistentDataPath, DbFileName);
        _db = new SQLiteConnection(DbPath);
        _db.CreateTable<Person>();
        Debug.Log($"[DB] Database initialized at: {DbPath}");
    }

    // ── CREATE ────────────────────────────────────────────────────────────────

    public Person Insert(string name, int age, string email)
    {
        var person = new Person { Name = name, Age = age, Email = email };
        _db.Insert(person);
        Debug.Log($"[DB] Inserted: {person}");
        return person;
    }

    // ── READ ──────────────────────────────────────────────────────────────────

    public List<Person> GetAll()
    {
        return _db.Table<Person>().ToList();
    }

    public Person GetById(int id)
    {
        return _db.Find<Person>(id);
    }

    // ── UPDATE ────────────────────────────────────────────────────────────────

    public bool Update(int id, string name, int age, string email)
    {
        var person = _db.Find<Person>(id);
        if (person == null) return false;

        person.Name  = name;
        person.Age   = age;
        person.Email = email;
        _db.Update(person);
        Debug.Log($"[DB] Updated: {person}");
        return true;
    }

    // ── DELETE ────────────────────────────────────────────────────────────────

    public bool Delete(int id)
    {
        var person = _db.Find<Person>(id);
        if (person == null) return false;

        _db.Delete(person);
        Debug.Log($"[DB] Deleted ID={id}");
        return true;
    }

    public void DeleteAll()
    {
        _db.DeleteAll<Person>();
        Debug.Log("[DB] All records deleted.");
    }

    public void Close()
    {
        _db?.Close();
    }
}
