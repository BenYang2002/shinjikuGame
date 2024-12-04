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

    // Game State
    private int totalPlayers = 4; // Total players in the lobby
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
            if (readyPlayers >= totalPlayers && readyPlayers >= 2)
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
        // Switch back to the lobby center (implementation depends on your scene management)
        Debug.Log("Back to Lobby Center");
        // Example: SceneManager.LoadScene("LobbyCenter");
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
}
