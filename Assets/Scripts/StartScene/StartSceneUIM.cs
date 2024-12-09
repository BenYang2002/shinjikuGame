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

    private GameClientAPI myapi = GameClientAPI.GetInstance();
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
            myapi.TCPListen();
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
        myapi = GameClientAPI.GetInstance();
        bool findServer = myapi.register();
        if(findServer)
        {
            Debug.Log("client has connected to server through TCP");
        }
        else
        {
            Debug.Log("client failed to connnect to server through TCP");
        }
        return findServer;
    }

    private void OnSubmitButtonPressed()
    {
        string username = usernameInputField.text;
        myapi.ThisUser.UserName = username; 
        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("Username cannot be empty.");
            return;
        }
        string prefix = new string("username");
        string message = prefix + " " + myapi.ThisUser.DeviceName + " " + username + " " + myapi.ThisUser.TcpPort + " " + myapi.ThisUser.UdpPort ;
        myapi.sendTCPMessage2Server(message);

        Debug.Log($"Username submitted: {message}");

        // Here, you can send the username to the server if needed
        // Example: SendUsernameToServer(username);

        // Load the Lobby scene
        SceneManager.LoadScene("LobbyScene"); // Ensure the Lobby scene is added in Build Settings
    }

}
