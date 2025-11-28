using UnityEngine;

public class WaspController : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotateSpeed = 5f;
    [SerializeField] private float stopDistance = 3f;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            animator.SetTrigger("Death");
        }

        if (target == null) return;

        Vector3 toTarget = target.position - transform.position;
        if (toTarget.magnitude <= stopDistance)
        {
            animator.SetTrigger("Attack");
            return;
        }

        //rotate
        Vector3 direction = toTarget.normalized;
        Quaternion lookRot = Quaternion.LookRotation(direction) * Quaternion.Euler(0f, 90f, 0f); //model specific offset
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, rotateSpeed * Time.deltaTime);

        //move
        transform.position += direction * speed * Time.deltaTime;
    }
}