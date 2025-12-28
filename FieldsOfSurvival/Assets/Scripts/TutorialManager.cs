using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public int step = 0;

    [Header("Scene Management")]
    public string mainGameSceneName = "MainGameScene";

    void Start()
    {
        UpdateText();
    }

    public void NextStep()
    {
        step++;
        UpdateText();
    }

    void UpdateText()
    {
        switch (step)
        {
            case 0:
                tutorialText.text =
                    "Use WASD to move\nPress Shift to sprint\nMove your mouse to look around\nUse spacebar to jump";
                break;

            case 1:
                tutorialText.text = "Press the left mouse button to shoot";
                break;

            case 2:
                tutorialText.text =
                    "Look at a field, select your vegetable, and press F to plant or harvest";
                break;

            case 3:
                tutorialText.text = "Walk to the barn to open the shop";
                break;

            case 4:
                tutorialText.text =
                    "Walk to one of the open gates and press B. If you have enough money, you will unlock a new plot to plant and harvest";
                break;

            case 5:
                tutorialText.text =
                    "Now look behind the barn. There will be 1 enemy. Kill it.";
                break;

            case 6:
                // Start countdown with visible timer
                StartCoroutine(LoadMainSceneAfterDelay(10f));
                break;
        }
    }

    public void OnTutorialEnemyKilled()
    {
        if (step == 5)
        {
            NextStep();
        }
    }

    // Countdown timer visible to player
    IEnumerator LoadMainSceneAfterDelay(float delay)
    {
        float remainingTime = delay;

        while (remainingTime > 0)
        {
            tutorialText.text = "Good job on completing the tutorial.\n" +
                                "You are ready to become a great farmer.\n" +
                                "Good luck out there.\n\n" +
                                $"Teleporting to main game in {Mathf.CeilToInt(remainingTime)} seconds...";
            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        SceneManager.LoadScene("MainScene");
    }
}
