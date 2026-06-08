using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// A single chat message row.
/// </summary>
public class ChatRowUI : MonoBehaviour
{
    public TMP_Text txtUser;
    public TMP_Text txtMessage;
    public TMP_Text txtTimestamp;
    public Image    rowBackground;

    private static readonly Color COLOR_SYSTEM = new Color(0.15f, 0.15f, 0.25f, 1f);
    private static readonly Color COLOR_NORMAL = new Color(0.10f, 0.10f, 0.18f, 1f);
    private static readonly Color COLOR_USER   = new Color(0.08f, 0.18f, 0.28f, 1f);

    public void Populate(string user, string text, string timestamp, bool isSystem)
    {
        if (txtUser)      txtUser.text      = isSystem ? "⚙ SYSTEM" : user;
        if (txtMessage)   txtMessage.text   = text;
        if (txtTimestamp) txtTimestamp.text  = timestamp;

        if (rowBackground != null)
            rowBackground.color = isSystem ? COLOR_SYSTEM : COLOR_NORMAL;

        if (txtUser != null)
            txtUser.color = isSystem
                ? new Color(0.6f, 0.6f, 0.7f)
                : new Color(0.4f, 0.8f, 1.0f);
    }
}
