using UnityEngine;
using API;
public class LobbyUIManager : MonoBehaviour
{
    // References to the canvases
    public GameObject lobbyCenterCanvas;
    public GameObject lobbyCreationCanvas;
    public void OnCreateLobbyButtonClicked()
    {
        if (lobbyCenterCanvas != null)
            lobbyCenterCanvas.SetActive(false);

        if (lobbyCreationCanvas != null)
            lobbyCreationCanvas.SetActive(true);
    }
}
