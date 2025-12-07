using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleMenu : MonoBehaviour
{
    private GameObject startScreen;
    private GameObject optionsMenu;
    private GameObject tutorialPrompt;

    void Awake()
    {
        startScreen = GameObject.Find("StartScreen");
        optionsMenu = GameObject.Find("OptionsMenu");
        tutorialPrompt = GameObject.Find("TutorialPrompt");

        if (!startScreen) Debug.LogError("StartScreen not found in hierarchy.");
        if (!optionsMenu) Debug.LogError("OptionsMenu not found in hierarchy.");
        if (!tutorialPrompt) Debug.LogError("TutorialPrompt not found in hierarchy.");

        startScreen.SetActive(true);
        optionsMenu.SetActive(false);
        tutorialPrompt.SetActive(false);
    }

    public void StartGame()
    {
        startScreen.SetActive(false);
        tutorialPrompt.SetActive(true);
    }

    public void OpenOptions()
    {
        startScreen.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        startScreen.SetActive(true);
    }

    public void SkipTutorial()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void StartTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
}
