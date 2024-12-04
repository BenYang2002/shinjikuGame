using UnityEngine;
using TMPro;

public class LobbyInfo : MonoBehaviour
{
    public string LobbyName; // Name of the lobby
    public int TotalPlayers; // Current number of players in the lobby
    public int ReadyPlayers; // Number of players marked as ready
    public int MaxPlayers; // Maximum allowed players
    public bool IsPrivate; // Whether the lobby is private or public

    public TextMeshProUGUI playerCountText; // Reference to the Player Count text
    public TextMeshProUGUI gameStatusText; // Reference to the Game Status text

    // Update the player count dynamically
    public void UpdatePlayerCount(int newTotalPlayers)
    {
        TotalPlayers = Mathf.Clamp(newTotalPlayers, 0, MaxPlayers);
        if (playerCountText != null)
        {
            playerCountText.text = $"Player Count: {TotalPlayers}/{MaxPlayers}";
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