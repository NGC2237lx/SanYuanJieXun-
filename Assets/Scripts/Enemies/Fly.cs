using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fly : Enemy
{
    public enum EnemyState
    {
        IDLE, HURT, DEAD, CHASING, ATTACKING // 新增ATTACKING状态
    }

    [SerializeField] private float movementHorizontalSpeed, movementVerticalSpeed;
    [SerializeField] private float flipIntervalTime;
    [SerializeField] private EnemyState currentState;
    [SerializeField] AudioClip enemyDamage;
    [SerializeField] AudioClip enemyDeathSword;
    [SerializeField] private float hurtForce, deadForce;
    [SerializeField] private float chaseSpeed = 10f;
    [SerializeField] private float chaseInterval = 3f;
    [SerializeField] private float randomFlyRange = 5f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float attackDuration = 1f; // 攻击动画持续时间

    private Transform player;
    private Rigidbody2D rb;
    private AudioSource audioPlayer;
    private HitEffect hit;
    private float lastFlipTime;
    private float lastChaseTime;
    private Vector2 randomFlyTarget;
    private bool isChasing = false;
    private bool canAttack = true; // 控制攻击冷却
    private float attackCooldown = 2f; // 攻击冷却时间

    private void Start()
    {
        canMove = true;

        rb = GetComponent<Rigidbody2D>();
        audioPlayer = GetComponent<AudioSource>();
        hit = GetComponentInChildren<HitEffect>();

        // 查找玩家对象
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 设置初始随机飞行目标
        SetRandomFlyTarget();
    }

    private void Update()
    {
        if (isDead)
            return;

        CheckIsDead();
        UpdateDirection();
        UpdateStatements();

        // 如果玩家在检测范围内且不在追逐状态，且冷却时间已过，则开始追逐
        if (player != null && !isChasing && Time.time > lastChaseTime + chaseInterval)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= detectionRange)
            {
                StartChase();
            }
        }

        // 攻击冷却处理
        if (!canAttack && Time.time > lastChaseTime + attackCooldown)
        {
            canAttack = true;
        }
    }

    private void FixedUpdate()
    {
        Movement();
    }

    private void SetRandomFlyTarget()
    {
        // 在当前位置周围随机设置一个飞行目标
        randomFlyTarget = (Vector2)transform.position + UnityEngine.Random.insideUnitCircle * randomFlyRange;
    }

    private void StartChase()
    {
        isChasing = true;
        SwitchState(EnemyState.CHASING);
        lastChaseTime = Time.time;
    }

    private void EndChase()
    {
        isChasing = false;
        SwitchState(EnemyState.IDLE);
        SetRandomFlyTarget(); // 设置新的随机飞行目标
    }

    private void UpdateStatements()
    {
        switch (currentState)
        {
            case EnemyState.IDLE:
                EnterIdleState();
                break;
            case EnemyState.CHASING:
                EnterChasingState();
                break;
            case EnemyState.ATTACKING: // 处理攻击状态
                EnterAttackingState();
                break;
        }
    }

    public void SwitchState(EnemyState state)
    {
        // 离开当前状态
        switch (currentState)
        {
            case EnemyState.IDLE:
                ExitIdleState();
                break;
            case EnemyState.HURT:
                ExitHurtState();
                break;
            case EnemyState.DEAD:
                ExitDeadState();
                break;
            case EnemyState.CHASING:
                ExitChasingState();
                break;
            case EnemyState.ATTACKING:
                ExitAttackingState();
                break;
        }

        // 进入新状态
        switch (state)
        {
            case EnemyState.IDLE:
                EnterIdleState();
                break;
            case EnemyState.HURT:
                EnterHurtState();
                break;
            case EnemyState.DEAD:
                EnterDeadState();
                break;
            case EnemyState.CHASING:
                EnterChasingState();
                break;
            case EnemyState.ATTACKING: // 新增攻击状态入口
                EnterAttackingState();
                break;
        }

        currentState = state;
    }

    private void EnterIdleState()
    {
        // 随机飞行行为
        if (Vector2.Distance(transform.position, randomFlyTarget) < 0.5f)
        {
            SetRandomFlyTarget();
        }
    }

    private void ExitIdleState()
    {
    }

    private void EnterChasingState()
    {
        // 追逐状态可以触发攻击
        if (canAttack && player != null && Vector2.Distance(transform.position, player.position) < chaseInterval)
        {
            SwitchState(EnemyState.ATTACKING);
            return;
        }

        // 否则继续追逐
        Invoke("EndChase", 2f);
    }

    private void ExitChasingState()
    {
    }

    // 新增攻击状态相关方法
    private void EnterAttackingState()
    {
        // 随机选择攻击动画
        int attackType = UnityEngine.Random.Range(0, 2); // 0或1
        animator.SetTrigger(attackType == 0 ? "Attack1" : "Attack2");

        // 设置攻击状态持续时间
        Invoke("EndAttack", attackDuration);
    }

    private void ExitAttackingState()
    {
        // 攻击结束后设置冷却时间
        canAttack = false;
        lastChaseTime = Time.time;
    }

    private void EndAttack()
    {
        // 攻击结束后回到追逐状态
        if (currentState == EnemyState.ATTACKING && !isDead)
        {
            SwitchState(EnemyState.CHASING);
        }
    }

    private void EnterHurtState()
    {
        hit.PlayHitAnimation();
        audioPlayer.PlayOneShot(enemyDamage);
        SwitchState(EnemyState.IDLE);
    }

    private void ExitHurtState()
    {
    }

    private void EnterDeadState()
    {
        hit.PlayHitAnimation();
        audioPlayer.PlayOneShot(enemyDeathSword);
        Vector3 diff = (player.position - transform.position).normalized;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 3;
        if (diff.x > 0)
        {
            rb.AddForce(Vector2.left * deadForce);
        }
        else if (diff.x < 0)
        {
            rb.AddForce(Vector2.right * deadForce);
        }
        animator.SetTrigger("Dead");
        Destroy(gameObject, 1f);
    }

    private void ExitDeadState()
    {
    }

    private void Movement()
    {
        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        switch (currentState)
        {
            case EnemyState.IDLE:
                // 随机飞行移动
                Vector2 directionToTarget = (randomFlyTarget - (Vector2)transform.position).normalized;
                rb.velocity = new Vector2(directionToTarget.x * Mathf.Abs(movementHorizontalSpeed),
                                        directionToTarget.y * Mathf.Abs(movementVerticalSpeed));
                break;

            case EnemyState.CHASING:
                if (player != null)
                {
                    // 向玩家突进
                    Vector2 chaseDirection = (player.position - transform.position).normalized;
                    rb.velocity = chaseDirection * chaseSpeed;

                    // 根据移动方向翻转图像
                    if (chaseDirection.x > 0 && transform.localScale.x > 0)
                    {
                        Flip();
                    }
                    else if (chaseDirection.x < 0 && transform.localScale.x < 0)
                    {
                        Flip();
                    }
                }
                break;

            case EnemyState.ATTACKING: // 攻击状态移动处理
                if (player != null)
                {
                    // 攻击时继续向玩家移动但速度减慢
                    Vector2 attackDirection = (player.position - transform.position).normalized;
                    rb.velocity = attackDirection * (chaseSpeed * 0.5f);
                }
                break;

            default:
                rb.velocity = new Vector2(movementHorizontalSpeed, movementVerticalSpeed);
                break;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time > lastFlipTime + flipIntervalTime)
        {
            lastFlipTime = Time.time;

            if (collision.contacts[0].normal.y != 0)
            {
                movementVerticalSpeed *= -1;
            }
            else if (collision.contacts[0].normal == Vector2.right || collision.contacts[0].normal == Vector2.left)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        // 翻转图像
        Vector3 vector = transform.localScale;
        vector.x *= -1;
        transform.localScale = vector;
    }

    protected override void UpdateDirection()
    {
        if (transform.lossyScale.x > 0 && movementHorizontalSpeed > 0)
        {
            isFacingLeft = false;
            movementHorizontalSpeed = Math.Abs(movementHorizontalSpeed) * -1;
        }
        else if (transform.lossyScale.x < 0 && movementHorizontalSpeed < 0)
        {
            isFacingLeft = true;
            movementHorizontalSpeed = Math.Abs(movementHorizontalSpeed);
        }
    }

    public override void Hurt(int damage, Transform attackPosition)
    {
        base.Hurt(damage, attackPosition);
        EnterHurtState();
    }

    protected override void Dead()
    {
        base.Dead();
        SwitchState(EnemyState.DEAD);
    }

    public bool GetDeadStatment()
    {
        return isDead;
    }

    // 调试可视化
    private void OnDrawGizmosSelected()
    {
        // 绘制状态范围
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, randomFlyRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制攻击范围（新增）
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseInterval); // 攻击触发距离
    }
}