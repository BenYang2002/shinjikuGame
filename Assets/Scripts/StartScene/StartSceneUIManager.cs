using UnityEngine;
using UnityEngine.UI;
using TMPro; // Use this if you're using TextMeshPro

public class UIManager : MonoBehaviour
{
    public GameObject usernamePanel; // Assign the Panel GameObject
    public TMP_InputField usernameInputField; // Assign the Input Field (or InputField for non-TMP)
    public Button submitButton; // Assign the Submit Button

    private void Start()
    {
        // Hide the username panel initially
        usernamePanel.SetActive(false);

        // Add listeners to buttons
        submitButton.onClick.AddListener(OnSubmit);
    }

    private void ShowUsernamePanel()
    {
        // Show the pop-up window
        usernamePanel.SetActive(true);
    }

    private void OnSubmit()
    {
        // Get the entered username
        string username = usernameInputField.text;

        if (string.IsNullOrEmpty(username))
        {
            Debug.Log("Please enter a username.");
            return;
        }

        Debug.Log("Username: " + username);

        // Hide the username panel after submission
        usernamePanel.SetActive(false);

        // Perform further actions with the username, e.g., starting the game
    }
}