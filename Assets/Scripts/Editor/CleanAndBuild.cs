using UnityEngine;
using UnityEditor;

public static class CleanAndBuild
{
    [MenuItem("Tools/Clean And Build SQLite Demo")]
    public static void Run()
    {
        // Remove old canvas if present
        var old = GameObject.Find("SQLiteDemo Canvas");
        if (old != null)
        {
            Object.DestroyImmediate(old);
            Debug.Log("[CleanAndBuild] Removed old canvas.");
        }

        SQLiteDemoSceneBuilder.Build();
    }
}
