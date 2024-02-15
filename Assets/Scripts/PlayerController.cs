using Shadow;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable Unity.InefficientPropertyAccess

public class PlayerController : MonoBehaviour
{
    [Header("玩法设置")] 
    [Tooltip("轻攻击")] public KeyCode lightKeyCode = KeyCode.J;
    [Tooltip("重攻击")] public KeyCode heavyKeyCode = KeyCode.K;
    [Tooltip("冲刺")] public KeyCode dashKeyCode = KeyCode.L;
    
    [Header("补偿速度")]
    [Tooltip("轻击补偿速度")] public float lightSpeed;
    [Tooltip("重击补偿速度")] public float heavySpeed;
    
    [Header("打击感")]
    public float shakeTime = 0.1f;
    public int lightPause = 6;
    public float lightStrength = 0.015f;
    public int heavyPause = 12;
    public float heavyStrength = 0.065f;

    [Header("攻击设置")]
    [Tooltip("攻击冷却")] public float interval = 2f;
    [Tooltip("移动速度")] public float moveSpeed = 5f;
    [Tooltip("跳跃力度")] public float jumpForce = 11f;
    
    [Header("冲锋相关设置")] 
    [Tooltip("冲刺持续时间")] public float dashTime = 0.15f;
    [Tooltip("冲刺冷却时间")] public float dashGapTime = 1f;
    [Tooltip("冲锋速度")] public float dashSpeed = 15f;
    [Tooltip("冲锋冷却显示UI")] public Image cdImage;
    
    [Header("其他设置")]
    [SerializeField] [Tooltip("地面的所处层级")] private LayerMask layer;
    [SerializeField] [Tooltip("角色与地面碰撞的检查范围")] private Vector3 check;
    
    private float dashStartTime = -10f;   // 上次冲锋的开始时间
    private float dashRestTime;    // 冲刺剩余时间
    
    private float timer;
    private bool isAttack;
    private string attackType;
    private int comboStep;

    
    private Rigidbody2D thisRigidbody;  // 刚体
    private Animator animator;  // 动画控制器
    private float horizontalInput;    // 水平输入量
    private bool isGround;  // 是否在地面上
    private bool isDash;   // 是否在冲锋状态
    
    
    // 动画索引
    private static readonly int Horizontal = Animator.StringToHash("Horizontal");
    private static readonly int Vertical = Animator.StringToHash("Vertical");
    private static readonly int IsGround = Animator.StringToHash("isGround");
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int LightAttack = Animator.StringToHash("LightAttack");
    private static readonly int ComboStep = Animator.StringToHash("ComboStep");
    private static readonly int HeavyAttack = Animator.StringToHash("HeavyAttack");

    private void Start()
    {
        thisRigidbody = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGround = Physics2D.OverlapCircle(transform.position + new Vector3(check.x, check.y, 0), check.z, layer);
        animator.SetFloat(Horizontal, thisRigidbody.velocity.x);
        animator.SetFloat(Vertical, thisRigidbody.velocity.y);
        animator.SetBool(IsGround, isGround);

        cdImage.fillAmount -= Time.deltaTime / dashGapTime;
        Dash();
        if (isDash) return;
        Move();
        Attack();
    }

    private void Move()
    {
        if (!isAttack)
            thisRigidbody.velocity = new Vector2(horizontalInput * moveSpeed, thisRigidbody.velocity.y);
        else
        {
            if (attackType == "Light")
                thisRigidbody.velocity = new Vector2(transform.localScale.x * lightSpeed, thisRigidbody.velocity.y);
            else if (attackType == "Heavy")
                thisRigidbody.velocity = new Vector2(transform.localScale.x * heavySpeed, thisRigidbody.velocity.y);
        }

        if (Input.GetButtonDown("Jump") && isGround)
        {
            thisRigidbody.velocity = new Vector2(0, jumpForce);
            animator.SetTrigger(Jump);
        }

        transform.localScale = thisRigidbody.velocity.x switch
        {
            < 0 => new Vector3(-1, 1, 1),
            > 0 => new Vector3(1, 1, 1),
            _ => transform.localScale
        };
    }

    private void Attack()
    {
        if (Input.GetKeyDown(lightKeyCode) && !isAttack)
        {
            isAttack = true;
            attackType = "Light";
            comboStep++;
            if (comboStep > 3)
                comboStep = 1;
            timer = interval;
            animator.SetTrigger(LightAttack);
            animator.SetInteger(ComboStep, comboStep);
        }
        if (Input.GetKeyDown(heavyKeyCode) && !isAttack)
        {
            isAttack = true;
            attackType = "Heavy";
            comboStep++;
            if (comboStep > 3)
                comboStep = 1;
            timer = interval;
            animator.SetTrigger(HeavyAttack);
            animator.SetInteger(ComboStep, comboStep);
        }


        if (timer != 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                comboStep = 0;
            }
        }
    }
    
    private void Dash()    // 冲锋的准备工作
    {
        if (isDash) // 目前正在冲锋
        {
            if (dashRestTime > 0) // 冲锋时间还没结束
            {
                thisRigidbody.velocity = new Vector2(horizontalInput * dashSpeed, thisRigidbody.velocity.y);
                dashRestTime -= Time.deltaTime;
                ShadowPool.Instance.Allocate(); // 生成影子
            }
            else
            {
                isDash = false;
            }
        }
        else if (Time.time >= dashStartTime + dashGapTime)   // 当CD冷却好了
        {
            if (Input.GetKeyDown(dashKeyCode))  // 按下冲锋键开启冲锋
            {
                isDash = true;
                cdImage.fillAmount = 1;
                dashStartTime = Time.time;  // 刷新开始时间
                dashRestTime = dashTime; // 刷新剩余冲锋时间
            }
        }
    }
    
    // Animation Event
    public void AttackOver()
    {
        isAttack = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (attackType == "Light")
            {
                AttackSense.Instance.HitPause(lightPause);
                AttackSense.Instance.CameraShake(shakeTime, lightStrength);
            }
            else if (attackType == "Heavy")
            {
                AttackSense.Instance.HitPause(heavyPause);
                AttackSense.Instance.CameraShake(shakeTime, heavyStrength);
            }

            if (transform.localScale.x > 0)
                other.GetComponent<Enemy>().GetHit(Vector2.right);
            else if (transform.localScale.x < 0)
                other.GetComponent<Enemy>().GetHit(Vector2.left);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position + new Vector3(check.x, check.y, 0), check.z);
    }
}