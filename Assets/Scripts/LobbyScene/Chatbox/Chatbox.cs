using UnityEngine;
using TMPro; // Required for TMP_InputField
using API;
public class Chatbox : MonoBehaviour
{
    public TMP_InputField chatInputField;  // Assign in Inspector
    public GameObject chatMessageTemplate;  // Assign the disabled text prefab
    public Transform chatContent;  // The Content GameObject inside the Scroll View

    // Method to send a chat message
    private GameClientAPI myapi;

    private void Start()
    {
        myapi = GameClientAPI.GetInstance();
        if(myapi.ChatConnected){
            myapi.storeDisplayMsg();
        }else{
            Debug.LogError("failed to connect to the chat server");
        }
    }
    private void Update()
    {
        // Process any received messages
        while (myapi.MessageQ.TryDequeue(out string message))
        {
            DisplayChatMessage(message);
        }
    }

        // Method to display a received chat message
    private void DisplayChatMessage(string message)
    {
        // Instantiate a new chat message
        GameObject newMessage = Instantiate(chatMessageTemplate, chatContent);
        newMessage.SetActive(true);

        // Set the text of the new message
        TextMeshProUGUI messageText = newMessage.GetComponent<TextMeshProUGUI>();
        if (messageText != null)
        {
            messageText.text = message;
            messageText.enabled = true;
        }

        // Scroll to the bottom
        Canvas.ForceUpdateCanvases();
        var scrollRect = chatContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
        scrollRect.verticalNormalizedPosition = 0f;
    }

    public void SendMessage()
    {
        // Check if the input field is not empty
        if (!string.IsNullOrEmpty(chatInputField.text))
        {
            // Instantiate a new chat message
            GameObject newMessage = Instantiate(chatMessageTemplate, chatContent);
            newMessage.SetActive(true);
            
            // Set the text of the new message
            TextMeshProUGUI messageText = newMessage.GetComponent<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = "You: " + chatInputField.text;
                messageText.enabled = true;
                string msg = "chat " + chatInputField.text; // add the chat prefix
                GameClientAPI.GetInstance().sendMessage2Chat(msg);
            }

            // Clear the input field
            chatInputField.text = "";

            // Scroll to the bottom
            Canvas.ForceUpdateCanvases();
            var scrollRect = chatContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
