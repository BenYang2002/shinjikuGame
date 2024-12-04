using UnityEngine;
using TMPro;

public class TugOfWarManager : MonoBehaviour
{
    public TextMeshProUGUI yourScoreText;     // Drag Your Score Text here
    public TextMeshProUGUI opponentScoreText; // Drag Opponent Score Text here
    public RectTransform playersImage;       // Drag the Players GameObject (Image) here

    public float moveStep = 10f;             // Amount the image moves per score
    public float maxDistance = 300f;         // Maximum distance before someone wins

    private int yourScore = 0;
    private int opponentScore = 0;
    private float currentPosition = 0f;      // Tracks the current horizontal position of the image

    void Update()
    {
        HandleInput();
        CheckWinCondition();
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Space)) // Player presses Space
        {
            yourScore++;
            currentPosition -= moveStep; // Move image left
            UpdateUI();
        }

        if (Input.GetKeyDown(KeyCode.Return)) // Opponent presses Enter
        {
            opponentScore++;
            currentPosition += moveStep; // Move image right
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        // Update score texts
        yourScoreText.text = $"Your Score: {yourScore}";
        opponentScoreText.text = $"Opponent Score: {opponentScore}";

        // Move the image horizontally
        playersImage.anchoredPosition = new Vector2(currentPosition, playersImage.anchoredPosition.y);
    }

    void CheckWinCondition()
    {
        if (Mathf.Abs(yourScore - opponentScore) >= 7)
        {
            string winner = yourScore > opponentScore ? "You Win!" : "Opponent Wins!";
            Debug.Log(winner);

            // Disable further input and show result
            yourScoreText.text = winner;
            opponentScoreText.text = "";
            enabled = false; // Stops the game
        }
    }
}
