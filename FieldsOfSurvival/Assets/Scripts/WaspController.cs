using UnityEngine;

public class WaspController : MonoBehaviour
{
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float stopDistance = 3f;

    [SerializeField] private int attackDamage = 3;

    private Animator animator;
    private Plant currentTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
        SetTarget();
    }

    void Update()
    {
        //animation transition
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Death");
        }

        if (currentTarget == null || currentTarget.IsDead())
        {
            SetTarget();
            if (currentTarget == null)
            {
                animator.SetBool("Attacking", false);
                return;
            }
        }

        Vector3 toTarget = currentTarget.transform.position - transform.position;

        if (toTarget.magnitude <= stopDistance)
        {
            animator.SetBool("Attacking", true);
            return;
        }
        else
        {
            animator.SetBool("Attacking", false);
        }

        //rotate
        Vector3 direction = toTarget.normalized;
        Quaternion lookRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f); //model specific offset
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, rotateSpeed * Time.deltaTime);

        //move
        transform.position += direction * speed * Time.deltaTime;
    }
    private void SetTarget()
    {
        currentTarget = FarmManager.Instance.GetClosestPlant(transform.position);
    }

    //Animation event
    private void DealDamageEvent()
    {
        if (currentTarget != null && !currentTarget.IsDead())
        {
            currentTarget.TakeDamage(attackDamage);
        }
    }
}