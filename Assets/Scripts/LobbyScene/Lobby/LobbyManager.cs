using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public GameObject lobbyPrefab; // Assign the LobbyEntry prefab here
    public Transform lobbyHolder; // Assign the container (Lobby Holder) in the LobbyCenter
    public int maxPlayers = 4;   // Default max player count for lobbies
    public GameObject lobbyCenterCanvas; // Reference to the Lobby Center Canvas
    public GameObject lobbyCanvas;       // Reference to the Lobby Canvas
    public GameLobbyManager gameLobbyManager; // Reference to the GameLobbyManager script

    public void CreateLobby(string lobbyName, bool hasPassword)
    {
        // Instantiate a new lobby entry
        GameObject newLobby = Instantiate(lobbyPrefab, lobbyHolder);

        // Assign lobby details to the prefab
        newLobby.transform.Find("LobbyName").GetComponent<TextMeshProUGUI>().text = "Lobby Name: " + lobbyName;
        newLobby.transform.Find("PlayerCount").GetComponent<TextMeshProUGUI>().text = $"Player Count: 0/{maxPlayers}";
        newLobby.transform.Find("PrivacyStatus").GetComponent<TextMeshProUGUI>().text = hasPassword ? "Privacy Status: Private" : "Privacy Status: Public";
        newLobby.transform.Find("GameStatus").GetComponent<TextMeshProUGUI>().text = "Game Status: Waiting";

        // Add the LobbyInfo component if not already present
        LobbyInfo lobbyInfo = newLobby.GetComponent<LobbyInfo>();
        if (lobbyInfo == null)
        {
            lobbyInfo = newLobby.AddComponent<LobbyInfo>();
        }

        // Initialize the LobbyInfo data
        lobbyInfo.LobbyName = lobbyName;
        lobbyInfo.TotalPlayers = 0;
        lobbyInfo.ReadyPlayers = 0;
        lobbyInfo.MaxPlayers = maxPlayers;
        lobbyInfo.IsPrivate = hasPassword;

        // Assign player count and ready count references to LobbyInfo for dynamic updates
        lobbyInfo.playerCountText = newLobby.transform.Find("PlayerCount").GetComponent<TextMeshProUGUI>();
        lobbyInfo.gameStatusText = newLobby.transform.Find("GameStatus").GetComponent<TextMeshProUGUI>();

        // Add OnClick listener to the lobby button
        Button lobbyButton = newLobby.GetComponent<Button>();
        if (lobbyButton != null)
        {
            // Pass the lobby instance for contextual updates
            lobbyButton.onClick.AddListener(() => OnLobbyClicked(newLobby, lobbyInfo));
        }

        newLobby.SetActive(true);

        Debug.Log($"Lobby '{lobbyName}' created successfully.");
    }

    private void OnLobbyClicked(GameObject lobby, LobbyInfo lobbyInfo)
    {
        Debug.Log($"Lobby '{lobbyInfo.LobbyName}' clicked.");

        // Increment the total players for this lobby
        lobbyInfo.TotalPlayers++;

        // Update the player count text
        lobbyInfo.playerCountText.text = $"Player Count: {lobbyInfo.TotalPlayers}/{lobbyInfo.MaxPlayers}";

        // Notify GameLobbyManager about the current lobby
        if (gameLobbyManager != null)
        {
            // Set the details in the GameLobbyManager for the selected lobby
            gameLobbyManager.SetLobbyDetails(lobbyInfo.TotalPlayers, lobbyInfo.ReadyPlayers);
        }
        else
        {
            Debug.LogError("GameLobbyManager is not assigned in the Inspector!");
        }

        // Hide the LobbyCenterCanvas
        if (lobbyCenterCanvas != null)
        {
            lobbyCenterCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("LobbyCenterCanvas is not assigned in the Inspector!");
        }

        // Show the LobbyCanvas
        if (lobbyCanvas != null)
        {
            lobbyCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("LobbyCanvas is not assigned in the Inspector!");
        }
    }
}
