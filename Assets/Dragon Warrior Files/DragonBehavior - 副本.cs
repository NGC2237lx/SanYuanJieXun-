using System.Collections;
using UnityEngine;

public class DragonBehavior1 : Enemy
{
    public enum DragonState { Idle, Patrol, Chase, Attack, Hurt, Dead }
    [Header("State Settings")]
    public DragonState currentState = DragonState.Idle; // Ĭ��״̬����Ϊ����
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
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    public float minPatrolTime = 2f;
    public float maxPatrolTime = 5f;
    private float stateTimer = 0;

    [Header("Attack Settings")]
    public GameObject fireballPrefab;
    public Transform firePoint;
    public float fireRate = 100f;
    public int fireDamage = 25;
    public LayerMask playerLayer;

    [Header("Combat Settings")]
    public int contactDamage = 15;
    public float hitFlashDuration = 0.1f;
    public Color hitFlashColor = Color.red;

    //��Ч
    [Header("Audio Clip")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] AudioClip[] deathSounds; // ������Ч
    [SerializeField] AudioClip hurtSound;    // ������Ч

    // ������˶�������ʱ��
    public float hurtAnimationTime = 0.5f;
    private float hurtTimeRemaining;

    [Header("Hit Effect")]
    [SerializeField] private HitEffect hitEffect; // �����ܻ���Ч���

    // Component References
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    // Internal Variables
    private Transform player;
    private Color originalColor;
    private Vector2 patrolCenter;
    private float nextFireTime;
    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Initialize properties
        originalColor = spriteRenderer.color;
        patrolCenter = transform.position;
        stateTimer = Random.Range(minIdleTime, maxIdleTime); // ��ʼ��Ϊ����ʱ��

        // Default setup
        if (firePoint == null) firePoint = transform;

        // ȷ������ƵԴ���
        if (audioSource == null)
        {
            audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                // �������
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.8f; // 3D��Ч����
            }
        }
    }
    // === �����¼�ʹ�õ���Ƶ���� ===
    public void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // === �������������Ч ===
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
            // ȷ�����л�������״̬
            if (currentState != DragonState.Dead)
                currentState = DragonState.Dead;
            return;
        }
        CheckIsDead();

        // ����״̬����
        if (currentState == DragonState.Dead)
        {
            animator.SetBool("Dead", true);
            return;
        }

        if (player == null) return;

        HandleHurtState(); // ��������״̬��ʱ

        if (currentState != DragonState.Hurt) // ����ʱ��ͣ�����߼�
        {
            UpdateState();
            HandleStateBehavior();
        }

        // Attack cooldown
        if (Time.time >= nextFireTime && currentState == DragonState.Attack)
        {
            LaunchFireball();
            nextFireTime = Time.time + fireRate;
        }
    }
    // === 1. ��дHurt���� ===
    // === �޸ĺ��Hurt���� ===
    public override void Hurt(int damage, Transform attackPosition)
    {
        if (isDead) return;

        base.Hurt(damage, attackPosition);

        // �˺��Ӿ�����
        if (hitEffect != null) hitEffect.PlayHitAnimation();
        StartCoroutine(FlashHitEffect());

        // �˺���������
        if (hurtSound != null && audioSource != null)
            audioSource.PlayOneShot(hurtSound);

        // ����������
        Vector2 knockbackDirection = (transform.position - attackPosition.position).normalized;
        ApplyKnockback(knockbackDirection, 300f);

        // ��������״̬
        currentState = DragonState.Hurt;
        hurtTimeRemaining = hurtAnimationTime;
        animator.SetTrigger("Hurt");
    }

    // === ��ӻ��˷��� ===
    private void ApplyKnockback(Vector2 direction, float force)
    {
        rb.velocity = Vector2.zero; // �����ǰ�ٶ�
        rb.AddForce(direction * force);
    }
    // === �����ɫ��˸Э�� ===
    private IEnumerator FlashHitEffect()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        spriteRenderer.color = originalColor;
    }
    // === 2. ��дDead���� ===
    protected override void Dead()
    {
        if (currentState == DragonState.Dead) return;
        base.Dead();
        // �л�������״̬
        if (currentState != DragonState.Dead)
        {
            currentState = DragonState.Dead;
            animator.SetTrigger("Dead");
        }
        // ����������Ч
        if (deathSounds != null && deathSounds.Length > 0)
        {
            PlayRandomDeathSound();
        }

        // ������ײ�������
        DisableCollidersAndPhysics();
    }

    private void DisableCollidersAndPhysics()
    {
        // ����������ײ��
        foreach (var collider in GetComponents<Collider2D>())
        {
            collider.enabled = false;
        }

        // ֹͣ�����˶�
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // ֹͣ������Ϊ
        enabled = false;
    }
    void HandleHurtState()
    {
        if (currentState == DragonState.Hurt)
        {
            hurtTimeRemaining -= Time.deltaTime;

            // �˺���������ʱ�����ʵ���״̬
            if (hurtTimeRemaining <= 0)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);

                // ������Ҿ���ѡ���ʵ�״̬
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

                // �������˼�ʱ��
                hurtTimeRemaining = 0;
            }
        }
    }

    void UpdateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // ״̬ת�����ȼ������� > ���� > ���� > ׷�� > Ѳ�� > ����
        // (������Update��ʼ����������Hurt������������ֻ�����Ծ״̬)

        // 1. ����ڹ�����Χ�� -> ����״̬�����ȼ���ߣ�
        if (distanceToPlayer <= attackRange && distanceToPlayer > minPlayerDistance)
        {
            if (currentState != DragonState.Attack)
            {
                currentState = DragonState.Attack;
                animator.SetTrigger("Attack");
            }
            return; // ֱ�ӷ��أ����������״̬
        }

        // 2. �����׷����Χ�� -> ׷��״̬���е����ȼ���
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState != DragonState.Chase)
            {
                currentState = DragonState.Chase;
                animator.SetBool("Chasing", true);
            }
            return; // ֱ�ӷ���
        }

        // 3. �����Ѳ�߷�Χ�� -> Ѳ��״̬��������ȼ���
        if (distanceToPlayer <= patrolRange)
        {
            if (currentState != DragonState.Patrol)
            {
                currentState = DragonState.Patrol;
                animator.SetBool("Chasing", false);
                stateTimer = Random.Range(minPatrolTime, maxPatrolTime);
            }
            return; // ֱ�ӷ���
        }

        // 4. ����������� -> ����״̬
        if (currentState != DragonState.Idle)
        {
            currentState = DragonState.Idle;
            animator.SetBool("Chasing", false);
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
        }
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
        // ����״̬��ʱ��
        stateTimer -= Time.deltaTime;

        // ȷ���ڿ���״̬�±��־�ֹ
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetFloat("Speed", 0);

        // ������ʱ����������ֿ���״̬
        // ����Ҫ�Զ�ת��Ѳ�ߣ���UpdateState����
    }

    void HandlePatrolState()
    {
        // ״̬��ʱ��
        stateTimer -= Time.deltaTime;

        // ��ʱ״̬ת��
        if (stateTimer <= 0)
        {
            // �л�������״̬
            currentState = DragonState.Idle;
            animator.SetBool("Chasing", false);
            stateTimer = Random.Range(minIdleTime, maxIdleTime);
            return;
        }

        // ���ǰ���ϰ�
        bool shouldTurn = false;

        // ���ǰ���Ƿ���ǽ��
        Collider2D wallCollider = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
        if (wallCollider != null)
        {
            shouldTurn = true;
        }

        // ���ǰ���Ƿ��е���
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        if (groundCollider == null)
        {
            shouldTurn = true;
        }

        // ��������ϰ��򳬳�Ѳ�߷�Χ������ת��
        if (shouldTurn && Time.time > lastTurnTime + turnCooldown)
        {
            TurnAround();
            stateTimer += 1.0f; // ��һЩ����Ѳ��ʱ��
        }

        // �ƶ��߼� - ����ҷ���Ѳ��
        Vector2 directionToPlayer = (player.position - transform.position).normalized;
        Vector2 moveDirection = Vector2.right * Mathf.Sign(directionToPlayer.x);
        transform.Translate(moveDirection * patrolSpeed * Time.deltaTime);
        animator.SetFloat("Speed", patrolSpeed);

        // ת������ҷ���
        if (moveDirection.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (moveDirection.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleChaseState()
    {
        // ȷ������ײǽ���������
        Collider2D wallCollider = Physics2D.OverlapCircle(wallCheck.position, 0.1f, groundLayer);
        Collider2D groundCollider = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);

        if (wallCollider != null || groundCollider == null)
        {
            TurnAround();
        }

        // ׷�����
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * chaseSpeed * Time.deltaTime);
        animator.SetFloat("Speed", chaseSpeed);

        // �������
        if (direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void HandleAttackState()
    {
        // ֹͣ�ƶ����������
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetFloat("Speed", 0);

        Vector2 direction = (player.position - transform.position).normalized;
        if (direction.x > 0 && transform.localScale.x < 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (direction.x < 0 && transform.localScale.x > 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    void TurnAround()
    {
        lastTurnTime = Time.time;
        transform.localScale = new Vector3(-transform.localScale.x, 1, 1);
    }

    // �ɶ����¼�����
    public void LaunchFireball()
    {
        if (fireballPrefab == null || firePoint == null || player == null)
            return;

        GameObject fireball = Instantiate(fireballPrefab, firePoint.position, firePoint.rotation);
        Fireball fireballScript = fireball.GetComponent<Fireball>();

        if (fireballScript != null)
        {
            fireballScript.damage = fireDamage;
            fireballScript.playerLayer = playerLayer;

            // ���㳯����ҵķ���
            Vector2 direction = (player.position - firePoint.position).normalized;
            fireballScript.SetDirection(direction);

            // ������ת��ƥ�䷽��
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            fireball.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // ���Կ��ӻ�
    private void OnDrawGizmosSelected()
    {
        // ����״̬��Χ
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, patrolRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minPlayerDistance);

        // ���Ƶ������
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }

        // ����ǽ�ڼ���
        if (wallCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wallCheck.position, 0.1f);
        }
    }
}