using System.Collections;
using UnityEngine;

public class DragonBehavior : Enemy
{
    public enum DragonState { Idle, Patrol, Chase, Attack, Hurt, Dead }
    [Header("State Settings")]
    public DragonState currentState = DragonState.Idle; // 默认状态设置为空闲
    public LayerMask groundLayer;
    public Transform groundCheck;
    public Transform wallCheck;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;
    public float patrolRange = 5f;
    public float detectionRange = 8f;
    public float attackRange = 6f;
    public float minPlayerDistance = 3f;
    public float turnCooldown = 0.5f;
    private float lastTurnTime = 0;

    [Header("Patrol Settings")]
    public float minIdleTime = 3f;
    public float maxIdleTime = 3f;
    public float minPatrolTime = 3f;
    public float maxPatrolTime = 3f;
    private float stateTimer = 0;

    [Header("Attack Settings")]
    public GameObject fireballPrefab;
    public Transform firePoint;

    [Header("Attack Settings")]
    [Tooltip("火球发射的最小间隔时间（秒）")]
    public float fireCooldown = 5f;  // 更合理默认值：1.5ff秒

    [Header("Animation Settings")]
    public float fireballDelay = 0f; // 动画开始到发射火球的延迟时间


    public int fireDamage = 25;
    public LayerMask playerLayer;

    [Header("Combat Settings")]
    public int contactDamage = 15;
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.red;

    //音效
    [Header("Audio Clip")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] AudioClip[] deathSounds; // 死亡音效
    [SerializeField] AudioClip hurtSound;    // 受伤音效

    // 添加受伤动画持续时间
    public float hurtAnimationTime = 0.5f;
    private float hurtTimeRemaining;

    [Header("Hit Effect")]
    [SerializeField] private HitEffect hitEffect; // 拖入受击特效组件

    // Component References
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Internal Variables
    private Transform player;
    private Color originalColor;
    private Vector2 patrolCenter;

    private bool canAttack = true;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Initialize properties
        originalColor = spriteRenderer.color;
        patrolCenter = transform.position;
        stateTimer = Random.Range(minIdleTime, maxIdleTime); // 初始化为空闲时间

        // Default setup
        if (firePoint == null) firePoint = transform;

        // 确保有音频源组件
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // 添加配置
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.8f; // 3D音效设置
            }
        }
    }
    // === 动画事件使用的音频方法 ===
    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // === 播放随机死亡音效 ===
    public void PlayRandomDeathSound()
    {
        if (deathSounds != null && deathSounds.Length > 0)
        {
            int index = Random.Range(0, deathSounds.Length);
            PlayOneShot(deathSounds[index]);
        }
    }

    void Update()
    {
        if (isDead)
        {
            // 确保已切换到死亡状态
            if (currentState != DragonState.Dead)
                currentState = DragonState.Dead;
            return;
        }
        CheckIsDead();

        // 死亡状态处理
        if (currentState == DragonState.Dead)
        {
            animator.SetTrigger("Dead");
            return;
        }

        if (player == null) return;

        HandleHurtState(); // 处理受伤状态计时

        //不处于受伤状态时，进行其他动作的逻辑
        if (currentState != DragonState.Hurt) // 受伤时暂停其他逻辑
        {
            UpdateState();
            HandleStateBehavior();
        }
    }
    // === 1. 重写Hurt方法 ===
    // === 修改后的Hurt方法 ===
    public override void Hurt(int damage, Transform attackPosition)
    {
        if (isDead) return;

        base.Hurt(damage, attackPosition);

        // 伤害视觉反馈
        if (hitEffect != null) hitEffect.PlayHitAnimation();
        StartCoroutine(FlashHitEffect());

        // 伤害声音反馈
        if (hurtSound != null && audioSource != null)
            audioSource.PlayOneShot(hurtSound);

        // 击退物理反馈
        Vector2 knockbackDirection = (transform.position - attackPosition.position).normalized;
        ApplyKnockback(knockbackDirection, 300f);

        // 进入受伤状态
        currentState = DragonState.Hurt;
        hurtTimeRemaining = hurtAnimationTime;
        animator.SetTrigger("Hurt");
    }

    // === 添加击退方法 ===
    private void ApplyKnockback(Vector2 direction, float force)
    {
        rb.velocity = Vector2.zero; // 清除当前速度
        rb.AddForce(direction * force);
    }
    // === 添加颜色闪烁协程 ===
    private IEnumerator FlashHitEffect()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }
    // === 2. 重写Dead方法 ===
    protected override void Dead()
    {
        if (currentState == DragonState.Dead) return;
        base.Dead();
        // 切换到死亡状态
        if (currentState != DragonState.Dead)
        {
            currentState = DragonState.Dead;
            animator.SetTrigger("Dead");
        }
        // 播放死亡音效
        if (deathSounds != null && deathSounds.Length > 0)
        {
            PlayRandomDeathSound();
        }

        // 禁用碰撞体和物理
        DisableCollidersAndPhysics();
    }

    private void DisableCollidersAndPhysics()
    {
        // 禁用所有碰撞体
        foreach (var collider in GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }

        // 停止物理运动
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // 停止所有行为
        enabled = false;
    }
    void HandleHurtState()
    {
        if (currentState == DragonState.Hurt)
        {
            hurtTimeRemaining -= Time.deltaTime;

            // 伤害动画结束时返回适当的状态
            if (hurtTimeRemaining <= 0)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                // 根据玩家距离选择适当状态
                if (distanceToPlayer <= attackRange && distanceToPlayer > minPlayerDistance)
                {
                    currentState = DragonState.Attack;
                }
                else if (distanceToPlayer <= detectionRange)
                {
                    currentState = DragonState.Chase;
                }
                else if (distanceToPlayer <= patrolRange)
                {
                    currentState = DragonState.Patrol;
                }
                else
                {
                    currentState = DragonState.Idle;
                }

                // 重置受伤计时器
                hurtTimeRemaining = 0;
            }
        }
    }

    void UpdateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // 状态转换优先级：死亡 > 受伤 > 攻击 > 追击 > 巡逻 > 空闲
        // (死亡在Update开始处理，受伤在Hurt方法处理，这里只处理活跃状态)

        // 1. 玩家在攻击范围内 -> 攻击状态（优先级最高）
        if (distanceToPlayer <= attackRange && distanceToPlayer > minPlayerDistance)
        {
            if (currentState != DragonState.Attack)
            {
                currentState = DragonState.Attack;
                animator.SetFloat("Speed", 0);
                animator.SetBool("Idle", false);
                animator.SetBool("Patrol", false);
                animator.SetBool("Chasing", false);

            }
            return; // 直接返回，不检查其他状态
        }

        // 2. 玩家在追击范围内 -> 追击状态（中等优先级）
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != DragonState.Chase)
            {
                currentState = DragonState.Chase;
                animator.SetFloat("Speed", chaseSpeed);
                animator.SetBool("Idle", false);
                animator.SetBool("Patrol", false);
                animator.SetBool("Chasing", true);
            }
            return; // 直接返回
        }

        // 3. 玩家在巡逻范围内 -> 巡逻状态（最低优先级）
        if (distanceToPlayer <= patrolRange)
        {
            if (currentState != DragonState.Patrol)
            {
                currentState = DragonState.Patrol;
                animator.SetFloat("Speed", patrolSpeed);
                animator.SetBool("Idle", false);
                animator.SetBool("Patrol", true);
                animator.SetBool("Chasing", false);
            }
            return; // 直接返回
        }

        // 4. 如果都不满足 -> 空闲状态
        if (currentState != DragonState.Idle)
        {
            currentState = DragonState.Idle;
            animator.SetFloat("Speed", 0);
            animator.SetBool("Idle", true);
            animator.SetBool("Patrol", false);
            animator.SetBool("Chasing", false);
        }
        HandleStateBehavior();
    }

    void HandleStateBehavior()
    {
        switch (currentState)
        {
            case DragonState.Idle:
                HandleIdleState();
                break;

            case DragonState.Patrol:
                HandlePatrolState();
                break;

            case DragonState.Chase:
                HandleChaseState();
                break;

            case DragonState.Attack:
                HandleAttackState();
                break;
        }
    }

    void HandleIdleState()
    {

        // 确保在空闲状态下保持静止
        rb.velocity = new Vector2(0, rb.velocity.y);
        
    }

    void HandlePatrolState()
    {
        // 检测前方障碍
        bool shouldTurn = false;

        // 检查前方是否有墙壁
        Collider2D wallCollider = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
        if (wallCollider != null)
        {
            shouldTurn = true;
        }

        // 检查前方是否有地面
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        if (groundCollider == null)
        {
            shouldTurn = true;
        }

        // 如果遇到障碍或超出巡逻范围，尝试转向
        if (shouldTurn && Time.time > lastTurnTime + turnCooldown)
        {
            TurnAround();
            stateTimer += 1.0f; // 给一些额外巡逻时间
        }
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 moveDirection = Vector2.right * Mathf.Sign(directionToPlayer.x);
        transform.Translate(moveDirection * patrolSpeed * Time.deltaTime);
        

        // 转向朝着玩家方向
        if (moveDirection.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        else if (moveDirection.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
    }

    void HandleChaseState()
    {
        // 确保不会撞墙或掉下悬崖
        Collider2D wallCollider = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        if (wallCollider != null || groundCollider == null)
        {
            TurnAround();
        }

        // 追踪玩家
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * chaseSpeed * Time.deltaTime);

        // 面向玩家
        if (direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        else if (direction.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);
    }

    void HandleAttackState()
    {
        //Debug.Log("进入攻击范围");
        // 停止移动并面向玩家
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetFloat("Speed", 0);

        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        else if (direction.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-1.5f, 1.5f, 1.5f);

        //在发射火球的冷却期过去后才可以发射
        if (canAttack) { 
            animator.SetTrigger("Attack");
            canAttack=false;
            StartCoroutine(ExecuteFireSequence());//发射完进入冷却器
        }
    }
    // 由动画事件调用的方法（必须在动画编辑器中添加到合适的帧）
    // 修改动画事件方法：只用于日志，不发射火球
    public void AnimEvent_LaunchFireball()
    {
        Debug.Log("动画播放到火球发射点");
    }

    //协程等待火球发射冷却时间
    private IEnumerator ExecuteFireSequence()
    {
        Debug.Log($"等待火球发射冷却: {fireCooldown}秒");
        yield return new WaitForSeconds(fireCooldown);

        canAttack = true;
    }


    void TurnAround()
    {
        lastTurnTime = Time.time;
        transform.localScale = new Vector3(-transform.localScale.x, 1.5f, 1.5f);
    }

    // 由动画事件调用
    public void LaunchFireball()
    {
        Debug.Log("发射火球");
        if (fireballPrefab == null || firePoint == null || player == null)
            return;

        // 获取火球预制体
        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
        Fireball fireballScript = fireball.GetComponent<Fireball>();

        if (fireballScript != null)
        {
            Vector3 PlayerHeadPosition = player.position + new Vector3(0, 1f, 0); // 玩家头部位置
            
            // 计算朝向玩家的方向
            //Vector2 direction = (player.position - firePoint.position).normalized;
            Vector2 direction = (PlayerHeadPosition - firePoint.position).normalized;
            // 设置火球速度（使用Fireball脚本中的速度）
            fireballScript.SetDirection(direction);

            // 传递伤害和层级
            fireballScript.damage = fireDamage;
            fireballScript.playerLayer = playerLayer;

            // 旋转火球方向
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            fireball.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        // 绘制状态范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minPlayerDistance);

        // 绘制地面检测点
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }

        // 绘制墙壁检测点
        if (wallCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }
    }
}