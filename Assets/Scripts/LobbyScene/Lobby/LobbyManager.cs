using UnityEngine;
using UnityEngine.UI;
using TMPro;
using API;
using NUnit.Framework;
using System.Collections.Generic;

public class LobbyManager : MonoBehaviour
{
    public GameObject lobbyPrefab; // Assign the LobbyEntry prefab here
    public Transform lobbyHolder; // Assign the container (Lobby Holder) in the LobbyCenter
    public int maxPlayers = 2;   // Default max player count for lobbies
    public GameObject lobbyCenterCanvas; // Reference to the Lobby Center Canvas
    public GameObject lobbyCanvas;       // Reference to the Lobby Canvas
    public GameLobbyManager gameLobbyManager; // Reference to the GameLobbyManager script
    private GameClientAPI myapi;
    private List<GameObject> lobbyList = new List<GameObject>();
    // Called when the "Create Lobby" button is clicked
    public void Start()
    {
        myapi = GameClientAPI.GetInstance();
    }
    public void Update()
    {
        if (myapi.UpdateLobbyList)
        {
            ClearLobbies();
            List<LobbyInfo> list = myapi.LobbyList;
            for (int i = 0; i < list.Count; i++) 
            {
                CreateLobby(list[i]);
            }
            myapi.UpdateLobbyList = false;
        }
    }
    private void ClearLobbies()
    {
        // Destroy each lobby GameObject in the list
        foreach (GameObject lobby in lobbyList)
        {
            Destroy(lobby);
        }

        // Clear the list to remove references to the destroyed objects
        lobbyList.Clear();
    }

    public void CreateLobby(LobbyInfo lobby)
    {
        // Instantiate a new lobby entry
        GameObject newLobby = Instantiate(lobbyPrefab, lobbyHolder);

        // Assign lobby details to the prefab
        newLobby.transform.Find("LobbyName").GetComponent<TextMeshProUGUI>().text = "Lobby Name: " + lobby.LobbyName;
        newLobby.transform.Find("PlayerCount").GetComponent<TextMeshProUGUI>().text = "Player Count: " + lobby.PlayerCount + "/" + maxPlayers;
        newLobby.transform.Find("PrivacyStatus").GetComponent<TextMeshProUGUI>().text = "Privacy Status: " + lobby.PrivacyStatus;
        newLobby.transform.Find("GameStatus").GetComponent<TextMeshProUGUI>().text = lobby.GameStatus;

        // Add the LobbyInfo component if not already present
        Lobby lobbyInfo = newLobby.GetComponent<Lobby>();
        if (lobbyInfo == null)
        {
            lobbyInfo = newLobby.AddComponent<Lobby>();
        }

        // Initialize the LobbyInfo data
        lobbyInfo.LobbyName = lobby.LobbyName;
        lobbyInfo.TotalPlayers = lobby.PlayerCount;
        lobbyInfo.ReadyPlayers = lobby.ReadyPlayerCount;
        lobbyInfo.MaxPlayers = maxPlayers;
        if (lobby.PrivacyStatus == "private")
        {
            lobbyInfo.IsPrivate = true;
        }
        else
        {
            lobbyInfo.IsPrivate = false;
        }
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
        lobbyList.Add(newLobby);
        newLobby.SetActive(true);

    }

    private void OnLobbyClicked(GameObject lobby, Lobby lobbyInfo)
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
