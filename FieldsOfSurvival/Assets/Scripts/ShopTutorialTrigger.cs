using UnityEngine;

public class ShopTutorialTrigger : MonoBehaviour
{
    public TutorialManager tutorial;

    private void OnTriggerEnter(Collider other)
    {
        if (tutorial == null) return;

        if (other.CompareTag("Player") && tutorial.step == 6)
        {
            tutorial.NextStep();
        }
    }
}
