using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Threading;
using API;

public class GameLobbyManager : MonoBehaviour
{
    // UI Elements
    public Button readyButton;
    public Button backButton;
    public Button cancelButton;
    public TextMeshProUGUI readyPlayerText; // Text showing "Number of players ready: X / Y"
    public GameObject lobbyCenterCanvas; // Reference to the LobbyCenterCanvas
    public GameObject lobbyCanvas;
    public GameClientAPI myapi;
    private bool shouldUpdate;
    public System.Object lockShouldUpdate = new System.Object();
    // Game State
    private LobbyInfo lobbyInfo;
    private bool isPlayerReady = false;
    public bool ShouldUpdate
    {
        get
        {
            lock (lockShouldUpdate)
            {
                return shouldUpdate;
            }
        }
        set
        {
            lock (lockShouldUpdate)
            {
                shouldUpdate = value;
            }
        }
    }
    public LobbyInfo LobbyInfo
    {
        get => lobbyInfo;
        set => lobbyInfo = value;
    }
    private void Update()
    {
        if (shouldUpdate)
        {
            for (int i = 0; i < myapi.LobbyList.Count; i++)
            {
                if (myapi.LobbyList[i].LobbyName == lobbyInfo.LobbyName)
                {
                    if (myapi.LobbyList[i].ReadyPlayerCount == 2)
                    {
                        BeginGame();
                    }
                    else
                    {
                        lobbyInfo = myapi.LobbyList[i];
                        UpdateReadyPlayerText();
                    }
                }
                break;
            }
            shouldUpdate = false;
        }
    }
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
        myapi = GameClientAPI.GetInstance();

        Debug.Log("GameLobbyManager initialized. Waiting for interactions.");
    }

    void UpdateReadyPlayerText()
    {
        readyPlayerText.text = $"Number of players ready: {lobbyInfo.ReadyPlayerCount} / {lobbyInfo.PlayerCount}";
    }

    void OnReadyButtonClicked()
    {
        if (!isPlayerReady)
        {
            // Player becomes ready
            myapi = GameClientAPI.GetInstance();
            isPlayerReady = true;
            string prefix = "lobbyModify";
            string name = lobbyInfo.LobbyName;
            string field = "readyplayercount";
            int val = ++lobbyInfo.ReadyPlayerCount;
            string value = val.ToString();
            myapi.sendTCPMessage2Server(prefix + " " + name + " " + field + " " + value);//notify the server about the change to broadcast updates

            // Update UI
            readyButton.interactable = false;
            backButton.interactable = false;
            cancelButton.gameObject.SetActive(true);

            // Update the ready player count
            UpdateReadyPlayerText();

            // Check if game can begin
            if (lobbyInfo.ReadyPlayerCount >= lobbyInfo.PlayerCount && lobbyInfo.ReadyPlayerCount >= 2)
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
            string prefix = "lobbyModify";
            string name = lobbyInfo.LobbyName;
            string field = "readyplayercount";
            int val = --lobbyInfo.ReadyPlayerCount;
            string value = val.ToString();
            myapi.sendTCPMessage2Server(prefix + " " + name + " " + field + " " + value);//notify the server about the change to broadcast updates

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
        myapi = GameClientAPI.GetInstance();
        // Decrease the player count
        if (lobbyInfo.PlayerCount > 0)
        {
            string prefix = "lobbyModify";
            string name = lobbyInfo.LobbyName;
            string field = "playercount";
            int val = --lobbyInfo.PlayerCount;
            string value = val.ToString();
            myapi.sendTCPMessage2Server(prefix + " " + name + " " + field + " " + value);//notify the server about the change to broadcast updates
            Debug.Log(prefix + " " + name + " " + field + " " + value);
            UpdateReadyPlayerText(); // Update the UI to reflect the new count
        }

        // Notify the LobbyManager about the updated player count (if needed)
        if (lobbyCenterCanvas != null)
        {
            // Update the player count in the lobby prefab
            /*Lobby currentLobbyInfo = FindObjectOfType<Lobby>(); // Assuming one lobby at a time
            if (currentLobbyInfo != null)
            {
                currentLobbyInfo.lobbyInfo.PlayerCount = totalPlayers; // Update the count in LobbyInfo
                currentLobbyInfo.playerCountText.text = $"Player Count: {totalPlayers}/{currentLobbyInfo.MaxPlayers}";
            }*/

            // Switch back to the Lobby Center Canvas
            lobbyCenterCanvas.SetActive(true);
        }

        // Then disable the current LobbyCanvas
        if (lobbyCanvas != null)
        {
            lobbyCanvas.SetActive(false);
        }
    }


    void BeginGame()
    {
        Debug.Log("All players are ready! Starting the game...");
        GameClientAPI myapi = GameClientAPI.GetInstance();
        // Ensure the target scene is added to the build settings
        string gameSceneName = "GameScene"; // Replace with your game scene's name
        int randomNumber = Random.Range(0, 1001);
        Thread.Sleep(randomNumber);
        myapi.sendTCPMessage2Server("startGame " + myapi.ThisUser.UserName + " "+ lobbyInfo.LobbyName);
        if (SceneManager.GetSceneByName(gameSceneName) != null)
        {
            // Load the game scene
            SceneManager.LoadScene(gameSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{gameSceneName}' not found! Make sure it is added to the build settings.");
        }
    }

    // Dynamically set the lobby details (e.g., total players)
    public void SetLobbyDetails(int newTotalPlayers, int initialReadyPlayers = 0)
    {
        lobbyInfo.PlayerCount = newTotalPlayers;
        lobbyInfo.ReadyPlayerCount = Mathf.Clamp(initialReadyPlayers, 0, lobbyInfo.PlayerCount); // Ensure readyPlayers is valid
        UpdateReadyPlayerText(); // Update UI to reflect new details
    }

    // Resets the lobby state when leaving
}
