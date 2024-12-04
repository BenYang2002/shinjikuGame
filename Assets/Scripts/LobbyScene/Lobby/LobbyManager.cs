using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    public GameObject lobbyPrefab; // Assign the LobbyEntry prefab here
    public Transform lobbyHolder; // Assign the container (Lobby Holder) in the LobbyCenter
    public int maxPlayers = 4;   // Default max player count for lobbies

    public void CreateLobby(string lobbyName, string password)
    {
        // Instantiate a new lobby entry
        GameObject newLobby = Instantiate(lobbyPrefab, lobbyHolder);

        // Assign lobby details to the prefab
        newLobby.transform.Find("LobbyName").GetComponent<TextMeshProUGUI>().text = "Lobby Name: " + lobbyName;
        newLobby.transform.Find("PlayerCount").GetComponent<TextMeshProUGUI>().text = $"Player Count: 1/{maxPlayers}";
        newLobby.transform.Find("PrivacyStatus").GetComponent<TextMeshProUGUI>().text = string.IsNullOrEmpty(password) ? "Privacy Status: Public" : "Privacy Status: Private";
        newLobby.transform.Find("GameStatus").GetComponent<TextMeshProUGUI>().text = "Game Status: Waiting";
        newLobby.SetActive(true);
    }
}
