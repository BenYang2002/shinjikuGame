using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameLobbyManager : MonoBehaviour
{
    // UI Elements
    public Button readyButton;
    public Button backButton;
    public Button cancelButton;
    public TextMeshProUGUI readyPlayerText; // Text showing "Number of players ready: X / Y"
    public GameObject lobbyCenterCanvas; // Reference to the LobbyCenterCanvas
    public GameObject lobbyCanvas;

    // Game State
    private int totalPlayers = 0; // Total players in the lobby
    private int readyPlayers = 0; // Number of players ready
    private bool isPlayerReady = false;

    void Start()
    {
        // Initialize the UI state
        cancelButton.gameObject.SetActive(false); // Hide the cancel button initially
        readyButton.interactable = true;
        backButton.interactable = true;

        // Add listeners for button clicks
        readyButton.onClick.AddListener(OnReadyButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
        backButton.onClick.AddListener(OnBackButtonClicked);

        // Update the initial ready player count
        UpdateReadyPlayerText();

        Debug.Log("GameLobbyManager initialized. Waiting for interactions.");
    }

    void UpdateReadyPlayerText()
    {
        readyPlayerText.text = $"Number of players ready: {readyPlayers} / {totalPlayers}";
    }

    void OnReadyButtonClicked()
    {
        if (!isPlayerReady)
        {
            // Player becomes ready
            isPlayerReady = true;
            readyPlayers++;

            // Update UI
            readyButton.interactable = false; // Disable ready button
            backButton.interactable = false; // Grey out back button
            cancelButton.gameObject.SetActive(true); // Show cancel button

            // Update the ready player count
            UpdateReadyPlayerText();

            // Check if game can begin
            if (readyPlayers >= totalPlayers && readyPlayers >= 1)
            {
                BeginGame();
            }
        }
    }

    void OnCancelButtonClicked()
    {
        if (isPlayerReady)
        {
            // Player cancels readiness
            isPlayerReady = false;
            readyPlayers--;

            // Update UI
            readyButton.interactable = true; // Enable ready button
            backButton.interactable = true; // Enable back button
            cancelButton.gameObject.SetActive(false); // Hide cancel button

            // Update the ready player count
            UpdateReadyPlayerText();
        }
    }

    void OnBackButtonClicked()
    {
        // Decrease the player count
        if (totalPlayers > 0)
        {
            totalPlayers--;
            UpdateReadyPlayerText(); // Update the UI to reflect the new count
        }

        // Notify the LobbyManager about the updated player count (if needed)
        if (lobbyCenterCanvas != null)
        {
            // Update the player count in the lobby prefab
            LobbyInfo currentLobbyInfo = FindObjectOfType<LobbyInfo>(); // Assuming one lobby at a time
            if (currentLobbyInfo != null)
            {
                currentLobbyInfo.TotalPlayers = totalPlayers; // Update the count in LobbyInfo
                currentLobbyInfo.playerCountText.text = $"Player Count: {totalPlayers}/{currentLobbyInfo.MaxPlayers}";
            }

            // Switch back to the Lobby Center Canvas
            lobbyCenterCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("LobbyCenterCanvas is not assigned in the Inspector!");
        }

        // Then disable the current LobbyCanvas
        if (lobbyCanvas != null)
        {
            lobbyCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError("LobbyCanvas is not assigned in the Inspector!");
        }

        Debug.Log("Player left the lobby. Total players decreased by 1.");
    }


    void BeginGame()
    {
        Debug.Log("All players are ready! Starting the game...");
        // Implement your game start logic here
    }

    // Call this method when a player enters the lobby
    public void PlayerEnteredLobby()
    {
        totalPlayers++; // Increment total players in the lobby
        UpdateReadyPlayerText(); // Update the UI to reflect the new count
    }

    // *** New Methods Below ***

    // Call this method when a player leaves the lobby
    public void PlayerLeftLobby()
    {
        if (totalPlayers > 0)
        {
            totalPlayers--; // Decrement total players
            if (readyPlayers > totalPlayers) readyPlayers = totalPlayers; // Adjust ready players if needed
            UpdateReadyPlayerText(); // Update the UI
        }
    }

    // Dynamically set the lobby details (e.g., total players)
    public void SetLobbyDetails(int newTotalPlayers, int initialReadyPlayers = 0)
    {
        totalPlayers = newTotalPlayers;
        readyPlayers = Mathf.Clamp(initialReadyPlayers, 0, totalPlayers); // Ensure readyPlayers is valid
        UpdateReadyPlayerText(); // Update UI to reflect new details
    }

    // Resets the lobby state when leaving
    public void ResetLobbyState()
    {
        // Reset game state
        isPlayerReady = false;
        readyPlayers = 0;

        // Reset UI elements
        readyButton.interactable = true;
        backButton.interactable = true;
        cancelButton.gameObject.SetActive(false); // Hide cancel button

        UpdateReadyPlayerText(); // Update ready player count text
    }
}
