using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialPlayerHelper : MonoBehaviour
{
    public TutorialManager tutorial;

    private void Update()
    {
        if (tutorial == null) return;

        switch (tutorial.step)
        {
            case 0: // movement
                float forward = (Keyboard.current.wKey.isPressed ? 1f : 0f) -
                                (Keyboard.current.sKey.isPressed ? 1f : 0f);
                float strafe = (Keyboard.current.dKey.isPressed ? 1f : 0f) -
                               (Keyboard.current.aKey.isPressed ? 1f : 0f);

                if (forward != 0 || strafe != 0)
                    tutorial.NextStep();
                break;

            case 1: // shooting
                if (Mouse.current.leftButton.wasPressedThisFrame)
                    tutorial.NextStep();
                break;

            case 2: // plant / harvest
                if (Keyboard.current.fKey.wasPressedThisFrame)
                    tutorial.NextStep();
                break;

            case 4: // unlock plot
                if (Keyboard.current.bKey.wasPressedThisFrame)
                    tutorial.NextStep();
                break;
        }
    }
}
