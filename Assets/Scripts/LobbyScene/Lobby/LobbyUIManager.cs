using UnityEngine;

public class LobbyUIManager : MonoBehaviour
{
    // References to the canvases
    public GameObject lobbyCenterCanvas;
    public GameObject lobbyCreationCanvas;

    // Called when the "Create Lobby" button is clicked
    public void OnCreateLobbyButtonClicked()
    {
        if (lobbyCenterCanvas != null)
            lobbyCenterCanvas.SetActive(false);

        if (lobbyCreationCanvas != null)
            lobbyCreationCanvas.SetActive(true);
    }
}
