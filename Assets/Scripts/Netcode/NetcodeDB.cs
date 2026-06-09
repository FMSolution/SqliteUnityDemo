using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SQLite;

/// <summary>
/// Lightweight SQLite wrapper used by both the server and client sides
/// of the Netcode sync demo.
/// Each instance opens its own .db file.
/// </summary>
public class NetcodeDB
{
    private SQLiteConnection _db;
    public string DbPath { get; private set; }

    public NetcodeDB(string fileName)
    {
        DbPath = Path.Combine(Application.persistentDataPath, fileName);
        _db    = new SQLiteConnection(DbPath);
        _db.CreateTable<SyncRecord>();
        Debug.Log($"[NetcodeDB] Opened: {DbPath}");
    }

    // ── Write ─────────────────────────────────────────────────────────────────

    /// <summary>Insert a record. Skips if the GUID already exists.</summary>
    public bool InsertIfNew(SyncRecord record)
    {
        if (_db.Find<SyncRecord>(r => r.Guid == record.Guid) != null)
            return false;   // already have it

        record.LocalId = 0; // let SQLite assign
        _db.Insert(record);
        return true;
    }

    /// <summary>Insert or replace a record by GUID.</summary>
    public void Upsert(SyncRecord record)
    {
        var existing = _db.Find<SyncRecord>(r => r.Guid == record.Guid);
        if (existing != null)
        {
            record.LocalId = existing.LocalId;
            _db.Update(record);
        }
        else
        {
            record.LocalId = 0;
            _db.Insert(record);
        }
    }

    // ── Read ──────────────────────────────────────────────────────────────────

    public List<SyncRecord> GetAll()      => _db.Table<SyncRecord>().ToList();
    public int              Count()       => _db.Table<SyncRecord>().Count();
    public void             DeleteAll()   => _db.DeleteAll<SyncRecord>();

    /// <summary>Add a brand-new record with a fresh GUID.</summary>
    public SyncRecord AddRecord(string origin, string key, string value)
    {
        var r = new SyncRecord
        {
            Guid      = System.Guid.NewGuid().ToString(),
            Origin    = origin,
            Key       = key,
            Value     = value,
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        _db.Insert(r);
        return r;
    }

    public void Close() => _db?.Close();
}
