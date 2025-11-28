using UnityEngine;

public class WaspController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Bijvoorbeeld: druk op spatie om te attacken
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Attack");
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Death");
        }
    }
}