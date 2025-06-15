using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy // 确保继承自 Enemy
{
    [Header("Skeleton Attr")]
    [SerializeField] public EnemyState state; // 当前骷髅兵的状态
    [SerializeField] private Transform groundCheck; // 用于地面检测的Transform
    [SerializeField] private Transform wallCheck; // 用于墙壁检测的Transform
    [SerializeField] private LayerMask whatIsGrounded; // 地面层
    [SerializeField] private LayerMask whatIsWall; // 墙壁层
    [SerializeField] public float speed = 1; // 移动速度
    [SerializeField] float groundCheckDistance = 0.2f; // 地面检测距离
    [SerializeField] float wallCheckDistance = 0.5f; // 墙壁检测距离

    [Header("Player Detection & Attack")]
    [SerializeField] public GameObject player; // 玩家对象 (可以直接从FindGameObjectWithTag获取)
    [SerializeField] float attackRange = 2f; // 触发普通攻击的距离
    [SerializeField] float comboRange = 3f; // 触发组合攻击的距离
    [SerializeField] private Transform attackPoint; // 攻击伤害检测的中心点 (子对象Transform)
    [SerializeField] private float attackRadius = 0.5f; // 攻击伤害检测的圆形范围
    [SerializeField] private LayerMask whatIsPlayer; // 玩家所在的图层

    [Header("Combat & Effects")]
    [SerializeField] float hurtForce = 400f; // 受击时的击退力
    [SerializeField] float deadForce = 50f; // 死亡时的击退力 (已调小)
    [SerializeField] private AudioSource audioSource; // 骷髅兵的音源
    [SerializeField] AudioClip[] attackSounds; // 攻击音效数组
    [SerializeField] AudioClip hurtSound; // 受击音效
    [SerializeField] private int attackDamage = 10; // 攻击能造成的伤害值

    // 私有成员变量
    private HitEffect hit; // 击中效果组件 (假定HitEffect是子对象上的组件)
    private Rigidbody2D rb; // 刚体组件
    private BoxCollider2D _boxCollider; // 骷髅兵的主碰撞体

    private int attackCount = 0; // 普通攻击计数器，用于轮换和触发组合攻击
    private int hurtCount = 0; // 受击计数器，用于触发组合攻击2
    private bool isAttacking = false; // 是否正在进行普通攻击 (由动画事件和状态机控制)
    private bool isComboing = false; // 是否正在进行组合攻击 (由动画事件和状态机控制)
    private bool groundDetected; // 地面检测结果
    private bool wallDetected; // 墙壁检测结果
    public bool playerDetectedInTrigger = false; // 用于OnTriggerEnter/Exit检测玩家

    public enum EnemyState
    {
        IDLE, // 待机
        WALK, // 移动
        ATTACK1, // 攻击1
        ATTACK2, // 攻击2
        HURT, // 受击
        DEATH, // 死亡 (修正了DEADTH拼写)
        Shield, // 盾牌 (如果需要)
        COMBO1, // 组合攻击1
        COMBO2 // 组合攻击2
    }

    #region Unity 生命周期方法

    // Start 在脚本实例被启用时在给定帧的第一次 Update 之前调用
    public void Start()
    {
        // 确保基类Enemy的Awake逻辑被调用，以便获取animator
        // 通常Enemy基类中的Awake会获取Animator，这里假设它已经在Start之前完成了

        rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();

        // 优化刚体物理设置
        if (rb != null)
        {
            rb.freezeRotation = true; // 防止意外旋转
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // 更精确的碰撞检测
            if (rb.sharedMaterial == null)
            {
                rb.sharedMaterial = new PhysicsMaterial2D()
                {
                    friction = 0.1f, // 摩擦力
                    bounciness = 0.05f // 弹性
                };
            }
        }
        else
        {
            Debug.LogError("Rigidbody2D not found on Skeleton. Movement will not work!", this);
        }

        hit = GetComponentInChildren<HitEffect>();
        if (hit == null)
        {
            Debug.LogWarning("HitEffect component not found in children of Skeleton. Hit animations may not play.", this);
        }

        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Player GameObject with tag 'Player' not found. Skeleton will not be able to follow or attack player.", this);
        }

        if (animator == null)
        {
            animator = GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogError("Animator component not found on Skeleton. Animations will not work!", this);
            }
        }

        SwitchState(EnemyState.IDLE); // 初始状态设为待机
    }

    // Update 每帧调用一次
    public void Update()
    {
         // 确保y位置始终为3
        if (transform.position.y != 3f)
        {
            transform.position = new Vector3(transform.position.x, 3f, transform.position.z);
        }
        if (isDead)
        {
            return;
        }

        CheckIsDead();
        Detect();

        UpdateAnimatorStatement();
    }

    // FixedUpdate 以固定的帧率调用，用于物理更新
    private void FixedUpdate()
    {
        if (isDead || rb == null)
        {
            return;
        }

        if (!isAttacking && !isComboing && state != EnemyState.HURT && state != EnemyState.DEATH)
        {
            UpdateDirection();
            HandleStateLogicFixedUpdate();
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }
    
    #endregion

    #region 状态逻辑和辅助方法

    // 在FixedUpdate中处理状态逻辑，因为它涉及Rigidbody
    private void HandleStateLogicFixedUpdate()
    {
        switch (state)
        {
            case EnemyState.IDLE:
                HandleIdleState();
                break;
            case EnemyState.WALK:
                HandleWalkState();
                break;
        }
    }

    // 处理待机状态的逻辑
    private void HandleIdleState()
    {
        if (rb != null) rb.velocity = Vector2.zero;

        if (player != null && playerDetectedInTrigger)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer > attackRange)
            {
                SwitchState(EnemyState.WALK);
            }
            else
            {
                DecideAttack();
            }
        }
    }

    // 处理移动状态的逻辑
    private void HandleWalkState()
    {
        if (player == null || !playerDetectedInTrigger)
        {
            SwitchState(EnemyState.IDLE);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= attackRange)
        {
            DecideAttack();
            return;
        }

        if (groundDetected && !wallDetected)
        {
            float moveDirection = isFacingLeft ? -1 : 1;
            if (rb != null)
            {
                rb.velocity = new Vector2(moveDirection * speed, rb.velocity.y);
            }
        }
        else
        {
            if (rb != null) rb.velocity = Vector2.zero;
            SwitchState(EnemyState.IDLE);
        }
    }

    // 检测地面和墙壁 (在Update中调用，因为射线检测不直接影响物理)
    private void Detect()
    {
        if (groundCheck == null || wallCheck == null)
        {
            Debug.LogError("GroundCheck or WallCheck Transform is not assigned in Skeleton script!", this);
            return;
        }

        RaycastHit2D groundHit = Physics2D.Raycast(groundCheck.position, Vector2.down,
                                                 groundCheckDistance, whatIsGrounded);
        groundDetected = groundHit.collider != null;

        Vector2 raycastDirection = isFacingLeft ? Vector2.left : Vector2.right;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, raycastDirection,
                                              wallCheckDistance, whatIsWall);
        wallDetected = wallHit.collider != null;
    }

    // 覆盖父类的 UpdateDirection，并在其中包含面向玩家的逻辑
    protected override void UpdateDirection()
    {
        base.UpdateDirection();

        if (player != null && !isAttacking && !isComboing && state != EnemyState.HURT && state != EnemyState.DEATH)
        {
            faceToPlayer();
        }
    }

    // 面向玩家
    public void faceToPlayer()
    {
        if (player != null)
        {
            bool playerIsLeft = player.transform.position.x < transform.position.x;
            if (playerIsLeft && !isFacingLeft)
            {
                Flip();
            }
            else if (!playerIsLeft && isFacingLeft)
            {
                Flip();
            }
        }
    }

    // 翻转图像
    void Flip()
    {
        Vector3 vector = transform.localScale;
        vector.x *= -1;
        transform.localScale = vector;
        isFacingLeft = !isFacingLeft;
    }

    // 更新动画状态机参数 (在Update中调用)
    private void UpdateAnimatorStatement()
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", state == EnemyState.WALK);
    }

    // 决定普通攻击或组合攻击 (修改此函数以优先处理combo2)
    private void DecideAttack()
    {
        if (isAttacking || isComboing)
        {
            return;
        }

        if (player == null || Vector2.Distance(transform.position, player.transform.position) > attackRange)
        {
            SwitchState(EnemyState.IDLE);
            return;
        }

        // 优先检查hurtCount是否满足combo2条件
        if (hurtCount >= 3 && Vector2.Distance(transform.position, player.transform.position) <= comboRange)
        {
            SwitchState(EnemyState.COMBO2);
            EnterComboState();
            hurtCount = 0;
            attackCount = 0;
        }
        else if (attackCount < 3)
        {
            SwitchState(state == EnemyState.ATTACK1 ? EnemyState.ATTACK2 : EnemyState.ATTACK1);
            EnterAttackState();
        }
        else
        {
            DecideComboAttack();
        }
    }

    // 决定组合攻击 (这个函数现在主要用于处理COMBO1，因为COMBO2已由DecideAttack优先处理)
    private void DecideComboAttack()
    {
        if (isAttacking || isComboing)
        {
            return;
        }

        if (player == null || Vector2.Distance(transform.position, player.transform.position) > comboRange)
        {
            SwitchState(EnemyState.IDLE);
            return;
        }

        // 现在只检查COMBO1的条件
        if (attackCount >= 3)
        {
            SwitchState(EnemyState.COMBO1);
            EnterComboState();
        }
        else
        {
            SwitchState(EnemyState.IDLE);
        }
    }

    // 进入普通攻击状态的实际执行
    private void EnterAttackState()
    {
        isAttacking = true;
        if (rb != null) rb.velocity = Vector2.zero;

        if (animator != null)
        {
            if (state == EnemyState.ATTACK1)
            {
                animator.SetTrigger("Attack1");
            }
            else if (state == EnemyState.ATTACK2)
            {
                animator.SetTrigger("Attack2");
            }
        }

        if (audioSource != null && attackSounds != null && attackSounds.Length > 0)
        {
            audioSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)]);
        }
        attackCount++;
    }

    // 进入组合攻击状态的实际执行
    private void EnterComboState()
    {
        isComboing = true;
        if (rb != null) rb.velocity = Vector2.zero;

        if (animator != null)
        {
            if (state == EnemyState.COMBO1)
            {
                animator.SetTrigger("Combo1");
            }
            else if (state == EnemyState.COMBO2)
            {
                animator.SetTrigger("Combo2");
            }
        }

        if (audioSource != null && attackSounds != null && attackSounds.Length > 0)
        {
            audioSource.PlayOneShot(attackSounds[UnityEngine.Random.Range(0, attackSounds.Length)]);
        }
        attackCount = 0;
        hurtCount = 0;
    }

    // ****** 以下方法将由 Animation Events 调用 ******

    // 在攻击动画特定帧调用，用于造成伤害
    public void DealDamage()
    {
        if (attackPoint == null || whatIsPlayer.value == 0)
        {
            Debug.LogWarning("攻击点或玩家层级未设置，无法造成伤害。");
            return;
        }

        // 尝试重新查找玩家，以防万一
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("未找到玩家对象，无法造成伤害。");
                return;
            }
        }

        // 执行碰撞检测
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsPlayer);

        foreach (Collider2D detectedCollider in hitColliders)
        {
            // 检查是否是玩家，并且获取 CharacterController2D 组件
            if (detectedCollider.gameObject.CompareTag("Player"))
            {
                CharacterController2D playerController = detectedCollider.GetComponent<CharacterController2D>();
                if (!player.GetComponent<CharacterData>().GetDeadStatement())
                {
                    // 调用玩家的 TakeDamage 方法，并传递敌人自身作为参数
                    StartCoroutine(character.TakeDamage(this));
                    FindObjectOfType<HitPause>().Stop(0.5f);
                    // 假设骷髅兵的攻击伤害就是 enemy.attackDamage
                    Debug.Log($"骷髅兵对玩家造成了 {attackDamage} 点伤害。");
                }
                else
                {
                    Debug.LogWarning($"检测到玩家 '{detectedCollider.name}' 但未找到 CharacterController2D 组件。");
                }
            }
        }
    }

    // 在普通攻击动画结束时由 Animation Event 调用
    public void ResetAttackStateFromAnimation()
    {
        isAttacking = false;
        SwitchState(EnemyState.IDLE);
    }

    // 在组合攻击动画结束时由 Animation Event 调用
    public void ResetComboStateFromAnimation()
    {
        isComboing = false;
        SwitchState(EnemyState.IDLE);
    }

    // 在受击动画结束时由 Animation Event 调用
    public void ResetHurtStateFromAnimation()
    {
        SwitchState(EnemyState.IDLE);
    }

    #endregion

    #region 覆盖基类方法

    // 覆盖Hurt方法，处理受击逻辑
    public override void Hurt(int damage, Transform attackPosition)
    {
        if (isComboing)
        {
            return;
        }

        base.Hurt(damage, attackPosition);
        CheckIsDead();

        if (!isDead)
        {
            hurtCount++;

            SwitchState(EnemyState.HURT);
            if (animator != null)
            {
                animator.SetTrigger("Hurt");
            }
            if (audioSource != null && hurtSound != null)
            {
                audioSource.PlayOneShot(hurtSound);
            }
            if (hit != null) hit.PlayHitAnimation();

            if (rb != null)
            {
                Vector2 knockbackDirection = (transform.position - attackPosition.position).normalized;
                rb.velocity = Vector2.zero;
                rb.AddForce(new Vector2(knockbackDirection.x * hurtForce, Mathf.Abs(knockbackDirection.y) * hurtForce * 0.5f), ForceMode2D.Impulse);
            }
        }
    }

    // 覆盖Dead方法，处理死亡逻辑
    protected override void Dead()
    {
        base.Dead();
        SwitchState(EnemyState.DEATH);
        StartCoroutine(DelayDead());
    }
    #endregion

    #region 死亡专属逻辑

    // 延迟死亡协程
    IEnumerator DelayDead()
    {
        if (hit != null) hit.PlayHitAnimation();
        if (animator != null) animator.SetTrigger("Dead");

        if (player != null && rb != null)
        {
            Vector3 diff = (transform.position - player.transform.position).normalized;
            rb.velocity = Vector2.zero;
            rb.AddForce(new Vector2(diff.x * deadForce, Mathf.Abs(diff.y) * deadForce * 0.5f), ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(1.5f);

        if (rb != null) rb.bodyType = RigidbodyType2D.Static;
        if (_boxCollider != null) _boxCollider.enabled = false;
    }
    #endregion

    // 状态切换方法，集中管理所有状态转换
    public void SwitchState(EnemyState newState)
    {
        if (state == newState)
        {
            return;
        }

        // 退出当前状态的清理逻辑
        switch (state)
        {
            case EnemyState.ATTACK1:
            case EnemyState.ATTACK2:
                isAttacking = false;
                break;
            case EnemyState.COMBO1:
            case EnemyState.COMBO2:
                isComboing = false;
                break;
            case EnemyState.WALK:
                if (rb != null) rb.velocity = Vector2.zero;
                break;
        }

        state = newState; // 更新当前状态

        // 针对某些状态，在进入时需要立即触发Animator Trigger (因为是Trigger参数，需要每次Set)
        if (animator != null)
        {
            switch (state)
            {
                case EnemyState.ATTACK1: animator.SetTrigger("Attack1"); break;
                case EnemyState.ATTACK2: animator.SetTrigger("Attack2"); break;
                case EnemyState.HURT: animator.SetTrigger("Hurt"); break;
                case EnemyState.DEATH: animator.SetTrigger("Dead"); break;
                case EnemyState.COMBO1: animator.SetTrigger("Combo1"); break;
                case EnemyState.COMBO2: animator.SetTrigger("Combo2"); break;
            }
        }
    }

    // 在编辑器中绘制Gizmos，用于调试
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
            Gizmos.DrawLine(groundCheck.position, new Vector2(groundCheck.position.x, groundCheck.position.y - groundCheckDistance));
        if (wallCheck != null)
        {
            Vector2 raycastDirection = isFacingLeft ? Vector2.left : Vector2.right;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)raycastDirection * wallCheckDistance);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, comboRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}