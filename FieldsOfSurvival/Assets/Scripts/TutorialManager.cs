using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    public TextMeshProUGUI tutorialText;

    [Header("Tutorial State")]
    public int step = 0;
    private bool waitingForAction = false;
    private bool timedTextRunning = false;

    [Header("Player Control")]
    public MonoBehaviour[] playerControlScripts;

    [Header("Scene Management")]
    public string mainGameSceneName = "MainScene";

    // Defensive items state
    private bool defensiveItemSelected = false;
    private int selectedDefensiveItem = 0; // 4,5,6
    private bool bearTrapPlaced = false;
    private bool fakeCropPlaced = false;
    private bool defensiveCropPlaced = false;

    void Start()
    {
        UpdateText();
    }

    void Update()
    {
        if (!waitingForAction) return;

        switch (step)
        {
            case 0: // Move (WASD)
                float forward = (Keyboard.current.wKey.isPressed ? 1f : 0f) -
                                (Keyboard.current.sKey.isPressed ? 1f : 0f);
                float strafe = (Keyboard.current.dKey.isPressed ? 1f : 0f) -
                               (Keyboard.current.aKey.isPressed ? 1f : 0f);
                if (forward != 0 || strafe != 0)
                    NextStep();
                break;

            case 1: // Sprint (Shift)
                if (Keyboard.current.leftShiftKey.wasPressedThisFrame)
                    NextStep();
                break;

            case 2: // Look around (mouse)
                if (Mathf.Abs(Mouse.current.delta.x.ReadValue()) > 0.1f ||
                    Mathf.Abs(Mouse.current.delta.y.ReadValue()) > 0.1f)
                    NextStep();
                break;

            case 3: // Jump (Space)
                if (Keyboard.current.spaceKey.wasPressedThisFrame)
                    NextStep();
                break;

            case 4: // Shoot (Left mouse)
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    NextStep();
                break;

            case 5: // Plant / Harvest
                if (Keyboard.current.fKey.wasPressedThisFrame ||
                    Keyboard.current.hKey.wasPressedThisFrame)
                    NextStep();
                break;

            case 6: // Enter shop (trigger handled externally)
                break;

            case 7: // Unlock plot (B)
                if (Keyboard.current.bKey.wasPressedThisFrame)
                    NextStep();
                break;

            case 8: // Defensive items
                if (!defensiveItemSelected)
                {
                    if (Keyboard.current.digit4Key.wasPressedThisFrame && !bearTrapPlaced)
                    {
                        defensiveItemSelected = true;
                        selectedDefensiveItem = 4;
                        tutorialText.text = "BearTrap selected. Press F to plant it";
                    }
                    else if (Keyboard.current.digit5Key.wasPressedThisFrame && !fakeCropPlaced)
                    {
                        defensiveItemSelected = true;
                        selectedDefensiveItem = 5;
                        tutorialText.text = "FakeCrop selected. Press F to plant it";
                    }
                    else if (Keyboard.current.digit6Key.wasPressedThisFrame && !defensiveCropPlaced)
                    {
                        defensiveItemSelected = true;
                        selectedDefensiveItem = 6;
                        tutorialText.text = "DefensiveCrop selected. Press F to plant it";
                    }
                }
                else
                {
                    if (Keyboard.current.fKey.wasPressedThisFrame)
                    {
                        // Mark the selected item as placed
                        if (selectedDefensiveItem == 4) bearTrapPlaced = true;
                        else if (selectedDefensiveItem == 5) fakeCropPlaced = true;
                        else if (selectedDefensiveItem == 6) defensiveCropPlaced = true;

                        defensiveItemSelected = false;
                        selectedDefensiveItem = 0;

                        // Give feedback
                        if (bearTrapPlaced && (!fakeCropPlaced || !defensiveCropPlaced))
                        {
                            tutorialText.text = "Great! Now try the other two defensive items";
                        }

                        // Advance to next step only if all three items are placed
                        if (bearTrapPlaced && fakeCropPlaced && defensiveCropPlaced)
                        {
                            NextStep();
                        }
                    }
                }
                break;

                // Step 9: enemy handled by coroutine / kill
        }
    }

    void UpdateText()
    {

        switch (step)
        {
            case 0:
                ShowActionText("Use WASD to move");
                break;
            case 1:
                ShowActionText("Press Shift to sprint");
                break;
            case 2:
                ShowActionText("Move your mouse to look around");
                break;
            case 3:
                ShowActionText("Press Space to jump");
                break;
            case 4:
                ShowActionText("Select your gun and press the left mouse button to shoot");
                break;
            case 5:
                ShowActionText("Select a vegetable, then plant crops with F and harvest with H");
                break;
            case 6:
                ShowActionText("Walk to the barn to open the shop");
                break;
            case 7:
                ShowActionText("Walk to an open space between the fences and press B to buy a new extension to your plot");
                break;
            case 8:
                ShowActionText("Select a defensive item: BearTrap (4), FakeCrop (5), DefensiveCrop (6), then press F to plant each one");
                break;
            case 9:
                if (!timedTextRunning)
                    StartCoroutine(EnemyIntroSequence());
                break;
            case 10:
                StartCoroutine(CongratulationSequence());
                break;
        }
    }

    void ShowActionText(string text)
    {
        tutorialText.text = text;
        waitingForAction = true;

    }

    IEnumerator EnemyIntroSequence()
    {
        timedTextRunning = true;


        tutorialText.text = "Look behind the barn";
        yield return new WaitForSeconds(1.5f);

        tutorialText.text = "There is an enemy";
        yield return new WaitForSeconds(1.5f);

        tutorialText.text = "KILL IT";
        waitingForAction = true;

        timedTextRunning = false;
    }

    // Called when the tutorial enemy is killed
    public void OnTutorialEnemyKilled()
    {
        if (step == 9)
            NextStep(); // move to step 10 for congratulation sequence
    }

    IEnumerator CongratulationSequence()
    {
        timedTextRunning = true;

        tutorialText.text = "Great job!";
        yield return new WaitForSeconds(2f);

        tutorialText.text = "You are ready to become a great farmer";
        yield return new WaitForSeconds(3f);

        tutorialText.text = "See you in the fields and try to survive ;)";
        yield return new WaitForSeconds(3f);

        // Trigger tutorial complete countdown
        StartCoroutine(LoadMainSceneAfterDelay(5f)); // 5 second countdown before loading main scene
        timedTextRunning = false;
    }


    public void NextStep()
    {
        step++;
        waitingForAction = false;
        UpdateText();
    }

    IEnumerator LoadMainSceneAfterDelay(float delay)
    {
        float remainingTime = delay;

        while (remainingTime > 0)
        {
            tutorialText.text =
                "Tutorial complete.\n\n" +
                "Teleporting to main game in " + Mathf.CeilToInt(remainingTime) + " seconds...";

            yield return new WaitForSeconds(1f);
            remainingTime -= 1f;
        }

        SceneManager.LoadScene(mainGameSceneName);
    }
}
