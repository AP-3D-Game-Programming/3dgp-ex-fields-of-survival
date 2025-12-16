using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;
    public int step = 0;

    [Header("Scene Management")]
    public string mainGameSceneName = "MainGameScene"; // set the name of your main game scene

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
                tutorialText.text = "Use WASD to move \nPress Shift to sprint \nMove your mouse to look around \nUse spacebar to jump";
                break;
            case 1:
                tutorialText.text = "Press the left mouse button to shoot";
                break;
            case 2:
                tutorialText.text = "Look at a field and select your vegetable and press F to plant/harvest";
                break;
            case 3:
                tutorialText.text = "Walk to the barn to open the shop";
                break;
            case 4:
                tutorialText.text = "Now Look Behind the barn. There wil be 1 enemy KILL IT.";
                break;
            case 5:
                // Tutorial complete: load main game scene
                SceneManager.LoadScene(mainGameSceneName);
                break;
        }
    }

    public void OnTutorialEnemyKilled()
    {
        if (step == 4)
        {
            NextStep();
        }
    }
}
