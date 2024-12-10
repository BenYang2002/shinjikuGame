using UnityEngine;
using TMPro;
using API;

public class Lobby : MonoBehaviour
{
    public LobbyInfo lobbyInfo;
    public int MaxPlayers = 2; // Current number of players in the lobby

    public TextMeshProUGUI playerCountText; // Reference to the Player Count text
    public TextMeshProUGUI gameStatusText; // Reference to the Game Status text
    public TextMeshProUGUI lobbyNameText; // Reference to the Game Status text
    public TextMeshProUGUI privacyStatus; // Reference to the Game Status text

    // Update the player count dynamically
    public void UpdatePlayerCount(int newTotalPlayers)
    {
        lobbyInfo.PlayerCount = Mathf.Clamp(newTotalPlayers, 0, MaxPlayers);
        if (playerCountText != null)
        {
            playerCountText.text = $"Player Count: {lobbyInfo.PlayerCount}/{MaxPlayers}";
        }
    }

    // Update the game status dynamically
    public void UpdateGameStatus(string newStatus)
    {
        if (gameStatusText != null)
        {
            gameStatusText.text = $"Game Status: {newStatus}";
        }
    }
}