using UnityEngine;
using Mirror;

public class PlayerColor : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnChangeColor))]
    public Color playerColor;

    public SpriteRenderer spriteRenderer;

    // An array of aesthetically pleasing colors
    private Color[] pleasingColors = {
        new Color(0.8f, 0.2f, 0.2f),  // Red
        new Color(0.2f, 0.8f, 0.2f),  // Green
        new Color(0.2f, 0.2f, 0.8f),  // Blue
        new Color(0.8f, 0.8f, 0.2f),  // Yellow
        new Color(0.8f, 0.2f, 0.8f),  // Magenta
        new Color(0.2f, 0.8f, 0.8f),   // Cyan
        new Color(0.7f, 0.2f, 0.2f),  // Red
        new Color(0.2f, 0.7f, 0.2f),  // Green
        new Color(0.2f, 0.2f, 0.7f),  // Blue
        new Color(0.7f, 0.7f, 0.2f),  // Yellow
        new Color(0.7f, 0.2f, 0.7f),  // Magenta
        new Color(0.2f, 0.7f, 0.7f)   // Cyan
    };

    // Called when the player object is instantiated on the client
    public override void OnStartClient()
    {
        InitializeSpriteRenderer();
        spriteRenderer.color = playerColor;
    }
    private void InitializeSpriteRenderer()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Called only on the server when the player joins the game
    public override void OnStartServer()
    {
        InitializeSpriteRenderer();
        int index = Random.Range(0, pleasingColors.Length);
        playerColor = pleasingColors[index];
    }

    // Called on the server and all clients whenever the playerColor SyncVar changes
    void OnChangeColor(Color oldColor, Color newColor)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
        else
        {
            Debug.LogWarning("SpriteRenderer is null.");
        }
    }
}
