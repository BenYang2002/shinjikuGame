using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LobbyCreationManager : MonoBehaviour
{
    // References to UI elements
    public TMP_InputField lobbyNameInput;     
    public TMP_InputField passwordInput;     
    public Toggle passwordToggle;            
    public TextMeshProUGUI lobbyNameReminder;
    public TextMeshProUGUI passwordReminder;  
    public GameObject lobbyCreationCanvas;    
    public GameObject lobbyCenterCanvas;     
    public Button submitButton;               
    public Button cancelButton;              
    public LobbyManager lobbyManager;         // Reference to the LobbyManager script

    private string lobbyPlaceholder = "Enter lobby name..."; // Placeholder for lobby name
    private string passwordPlaceholder = "Enter password..."; // Placeholder for password

    void Start()
    {
        // Add listeners to the toggle and buttons
        passwordToggle.onValueChanged.AddListener(OnPasswordToggleChanged);
        submitButton.onClick.AddListener(OnSubmit);
        cancelButton.onClick.AddListener(OnCancel);

        // Initially hide reminder texts and disable password input
        ResetInputs();
    }

    // Toggle password input visibility
    private void OnPasswordToggleChanged(bool isOn)
    {
        passwordInput.gameObject.SetActive(isOn);
    }

    // Handle submit button click
    private void OnSubmit()
    {
        bool isValid = true;

        // Validate lobby name
        if (string.IsNullOrEmpty(lobbyNameInput.text) || lobbyNameInput.text == lobbyPlaceholder)
        {
            lobbyNameReminder.gameObject.SetActive(true);
            isValid = false;
        }
        else
        {
            lobbyNameReminder.gameObject.SetActive(false);
        }

        // Validate password if toggle is enabled
        if (passwordToggle.isOn)
        {
            if (string.IsNullOrEmpty(passwordInput.text) || passwordInput.text == passwordPlaceholder)
            {
                passwordReminder.gameObject.SetActive(true);
                isValid = false;
            }
        }

        // If valid, output data and switch canvases
        if (isValid)
        {
            Debug.Log($"Lobby Name: {lobbyNameInput.text}");
            if (passwordToggle.isOn)
            {
                Debug.Log($"Password: {passwordInput.text}");
            }

            // Switch canvases
            lobbyCreationCanvas.SetActive(false);
            lobbyCenterCanvas.SetActive(true);
            lobbyManager.CreateLobby(lobbyNameInput.text, passwordInput.text);
            // Reset inputs after switching back
            ResetInputs();
        }
    }

    // Handle cancel button click
    private void OnCancel()
    {
        // Switch canvases
        lobbyCreationCanvas.SetActive(false);
        lobbyCenterCanvas.SetActive(true);

        // Reset inputs after switching back
        ResetInputs();
    }

    // Reset input fields and placeholders
    private void ResetInputs()
    {
        // Clear input fields
        lobbyNameInput.text = string.Empty;
        passwordInput.text = string.Empty;

        // Hide reminder texts
        lobbyNameReminder.gameObject.SetActive(false);
        passwordReminder.gameObject.SetActive(false);

        // Hide password input field if toggle is off
        passwordToggle.isOn = false;
        passwordInput.gameObject.SetActive(false);
    }
}
