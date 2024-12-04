using UnityEngine;
using UnityEngine.UI;
using TMPro; // Required if using TextMeshPro
using UnityEngine.SceneManagement;
using System.Net.Sockets;
using System.Threading;
using API;

public class StartButtonHandler : MonoBehaviour
{
    public Button startButton; // Assign the Start Button in the Inspector
    public GameObject usernamePanel; // Assign the Username Panel in the Inspector
    public TMP_InputField usernameInputField; // Assign the Username Input Field in the Inspector
    public Button submitButton; // Assign the Submit Button in the Inspector
    public GameObject startBackground;

    private GameClientAPI myapi;
    private TcpClient clientSocket;

    private void Start()
    {
        // Add listeners for the buttons
        startButton.onClick.AddListener(OnStartButtonPressed);
        submitButton.onClick.AddListener(OnSubmitButtonPressed);

        // Initially hide the Username Panel
        usernamePanel.SetActive(false);
    }

    private void OnStartButtonPressed()
    {
        // Disable the Start Button
        startButton.interactable = false;

        // Attempt to connect to the server
        if (ConnectToServer())
        {
            Debug.Log("Connected to the server!");
            // Show the Username Panel
            startBackground.SetActive(false);
            startButton.gameObject.SetActive(false);
            usernamePanel.SetActive(true);
        }
        else
        {
            Debug.LogError("Failed to connect to the server.");
            startButton.gameObject.SetActive(true);
            startButton.interactable = true; // Re-enable the Start Button if connection fails
        }
    }

    private bool ConnectToServer()
    {
        myapi = GameClientAPI.GetInstance(2000,3000);
        bool findServer = myapi.register();
        if(findServer){
            Thread udpListener = new Thread(myapi.UDPListen);
            udpListener.IsBackground = true;
            udpListener.Start();
        }
        Debug.Log("myapi.ChatConnected" + myapi.ChatConnected);
        return findServer;
    }

    private void OnSubmitButtonPressed()
    {
        string username = usernameInputField.text;

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }
        myapi.sendMessage2Chat(username);

        Debug.Log($"Username submitted: {username}");

        // Here, you can send the username to the server if needed
        // Example: SendUsernameToServer(username);

        // Load the Lobby scene
        SceneManager.LoadScene("LobbyScene"); // Ensure the Lobby scene is added in Build Settings
    }

    // Optional: Method to send username to the server
    private void SendUsernameToServer(string username)
    {
        try
        {
            if (clientSocket != null && clientSocket.Connected)
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(username);
                clientSocket.GetStream().Write(buffer, 0, buffer.Length);
                Debug.Log("Username sent to the server.");
            }
        }
        catch (SocketException ex)
        {
            Debug.LogError($"Failed to send username: {ex.Message}");
        }
    }
}
