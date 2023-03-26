using FSM;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    private Vector2 direction;
    private bool isHit;
    private AnimatorStateInfo info;

    private Animator animator;
    private Animator hitAnimator;
    private Rigidbody2D thisRigidbody2D;
    
    private static readonly int Hit = Animator.StringToHash("Hit");

    private void Start()
    {
        animator = transform.GetComponent<Animator>();
        hitAnimator = transform.GetChild(0).GetComponent<Animator>();
        thisRigidbody2D = transform.GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        info = animator.GetCurrentAnimatorStateInfo(0);
        if (isHit)
        {
            thisRigidbody2D.velocity = direction * speed;
            if (info.normalizedTime >= .6f)
            {
                isHit = false;
            }
        }
    }

    public void GetHit(Vector2 direction1)
    {
        transform.localScale = new Vector3(-direction1.x, 1, 1);
        isHit = true;
        direction = direction1;
        // animator.SetTrigger(Hit);
        GetComponent<FSMManager>().parameter.getHit = isHit;    // 使用状态机来切换
        hitAnimator.SetTrigger(Hit);
    }
}
