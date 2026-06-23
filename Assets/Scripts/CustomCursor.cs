using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    // Assign your Texture2D in the Unity Inspector
    [SerializeField] private Texture2D cursorTexture;

    // Choose the click point (Vector2.zero targets the top-left pixel)
    private Vector2 hotSpot = Vector2.zero;

    void Start()
    {
        // Applies the custom cursor right when the game starts
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }

    // Optional: Call this to reset back to the default system cursor
    public void ResetCursor()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
