using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : Enemy // ȷ���̳��� Enemy
{
    [Header("Skeleton Attr")]
    [SerializeField] public EnemyState state; // ��ǰ���ñ���״̬
    [SerializeField] private Transform groundCheck; // ���ڵ������Transform
    [SerializeField] private Transform wallCheck; // ����ǽ�ڼ���Transform
    [SerializeField] private LayerMask whatIsGrounded; // �����
    [SerializeField] private LayerMask whatIsWall; // ǽ�ڲ�
    [SerializeField] public float speed = 1; // �ƶ��ٶ�
    [SerializeField] float groundCheckDistance = 0.2f; // ���������
    [SerializeField] float wallCheckDistance = 0.5f; // ǽ�ڼ�����

    [Header("Player Detection & Attack")]
    [SerializeField] public GameObject player; // ��Ҷ��� (����ֱ�Ӵ�FindGameObjectWithTag��ȡ)
    [SerializeField] float attackRange = 2f; // ������ͨ�����ľ���
    [SerializeField] float comboRange = 3f; // ������Ϲ����ľ���
    [SerializeField] private Transform attackPoint; // �����˺��������ĵ� (�Ӷ���Transform)
    [SerializeField] private float attackRadius = 0.5f; // �����˺�����Բ�η�Χ
    [SerializeField] private LayerMask whatIsPlayer; // ������ڵ�ͼ��

    [Header("Combat & Effects")]
    [SerializeField] float hurtForce = 400f; // �ܻ�ʱ�Ļ�����
    [SerializeField] float deadForce = 50f; // ����ʱ�Ļ����� (�ѵ�С)
    [SerializeField] private AudioSource audioSource; // ���ñ�����Դ
    [SerializeField] AudioClip[] attackSounds; // ������Ч����
    [SerializeField] AudioClip hurtSound; // �ܻ���Ч
    [SerializeField] private int attackDamage = 10; // ��������ɵ��˺�ֵ

    // ˽�г�Ա����
    private HitEffect hit; // ����Ч����� (�ٶ�HitEffect���Ӷ����ϵ����)
    private Rigidbody2D rb; // �������
    private BoxCollider2D _boxCollider; // ���ñ�������ײ��

    private int attackCount = 0; // ��ͨ�����������������ֻ��ʹ�����Ϲ���
    private int hurtCount = 0; // �ܻ������������ڴ�����Ϲ���2
    private bool isAttacking = false; // �Ƿ����ڽ�����ͨ���� (�ɶ����¼���״̬������)
    private bool isComboing = false; // �Ƿ����ڽ�����Ϲ��� (�ɶ����¼���״̬������)
    private bool groundDetected; // ��������
    private bool wallDetected; // ǽ�ڼ����
    public bool playerDetectedInTrigger = false; // ����OnTriggerEnter/Exit������

    public enum EnemyState
    {
        IDLE, // ����
        WALK, // �ƶ�
        ATTACK1, // ����1
        ATTACK2, // ����2
        HURT, // �ܻ�
        DEATH, // ���� (������DEADTHƴд)
        Shield, // ���� (�����Ҫ)
        COMBO1, // ��Ϲ���1
        COMBO2 // ��Ϲ���2
    }

    #region Unity �������ڷ���

    // Start �ڽű�ʵ��������ʱ�ڸ���֡�ĵ�һ�� Update ֮ǰ����
    public void Start()
    {
        // ȷ������Enemy��Awake�߼������ã��Ա��ȡanimator
        // ͨ��Enemy�����е�Awake���ȡAnimator������������Ѿ���Start֮ǰ�����

        rb = GetComponent<Rigidbody2D>();
        _boxCollider = GetComponent<BoxCollider2D>();

        // �Ż�������������
        if (rb != null)
        {
            rb.freezeRotation = true; // ��ֹ������ת
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // ����ȷ����ײ���
            if (rb.sharedMaterial == null)
            {
                rb.sharedMaterial = new PhysicsMaterial2D()
                {
                    friction = 0.1f, // Ħ����
                    bounciness = 0.05f // ����
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

        SwitchState(EnemyState.IDLE); // ��ʼ״̬��Ϊ����
    }

    // Update ÿ֡����һ��
    public void Update()
    {
         // ȷ��yλ��ʼ��Ϊ3
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

    // FixedUpdate �Թ̶���֡�ʵ��ã������������
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

    #region ״̬�߼��͸�������

    // ��FixedUpdate�д���״̬�߼�����Ϊ���漰Rigidbody
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

    // �������״̬���߼�
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

    // �����ƶ�״̬���߼�
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

    // �������ǽ�� (��Update�е��ã���Ϊ���߼�ⲻֱ��Ӱ������)
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

    // ���Ǹ���� UpdateDirection���������а���������ҵ��߼�
    protected override void UpdateDirection()
    {
        base.UpdateDirection();

        if (player != null && !isAttacking && !isComboing && state != EnemyState.HURT && state != EnemyState.DEATH)
        {
            faceToPlayer();
        }
    }

    // �������
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

    // ��תͼ��
    void Flip()
    {
        Vector3 vector = transform.localScale;
        vector.x *= -1;
        transform.localScale = vector;
        isFacingLeft = !isFacingLeft;
    }

    // ���¶���״̬������ (��Update�е���)
    private void UpdateAnimatorStatement()
    {
        if (animator == null) return;
        animator.SetBool("IsWalking", state == EnemyState.WALK);
    }

    // ������ͨ��������Ϲ��� (�޸Ĵ˺��������ȴ���combo2)
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

        // ���ȼ��hurtCount�Ƿ�����combo2����
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

    // ������Ϲ��� (�������������Ҫ���ڴ���COMBO1����ΪCOMBO2����DecideAttack���ȴ���)
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

        // ����ֻ���COMBO1������
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

    // ������ͨ����״̬��ʵ��ִ��
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

    // ������Ϲ���״̬��ʵ��ִ��
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

    // ****** ���·������� Animation Events ���� ******

    // �ڹ��������ض�֡���ã���������˺�
    public void DealDamage()
    {
        if (attackPoint == null || whatIsPlayer.value == 0)
        {
            Debug.LogWarning("���������Ҳ㼶δ���ã��޷�����˺���");
            return;
        }

        // �������²�����ң��Է���һ
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("δ�ҵ���Ҷ����޷�����˺���");
                return;
            }
        }

        // ִ����ײ���
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, whatIsPlayer);

        foreach (Collider2D detectedCollider in hitColliders)
        {
            // ����Ƿ�����ң����һ�ȡ CharacterController2D ���
            if (detectedCollider.gameObject.CompareTag("Player"))
            {
                CharacterController2D playerController = detectedCollider.GetComponent<CharacterController2D>();
                if (!player.GetComponent<CharacterData>().GetDeadStatement())
                {
                    // ������ҵ� TakeDamage �����������ݵ���������Ϊ����
                    StartCoroutine(character.TakeDamage(this));
                    FindObjectOfType<HitPause>().Stop(0.5f);
                    // �������ñ��Ĺ����˺����� enemy.attackDamage
                    Debug.Log($"���ñ����������� {attackDamage} ���˺���");
                }
                else
                {
                    Debug.LogWarning($"��⵽��� '{detectedCollider.name}' ��δ�ҵ� CharacterController2D �����");
                }
            }
        }
    }

    // ����ͨ������������ʱ�� Animation Event ����
    public void ResetAttackStateFromAnimation()
    {
        isAttacking = false;
        SwitchState(EnemyState.IDLE);
    }

    // ����Ϲ�����������ʱ�� Animation Event ����
    public void ResetComboStateFromAnimation()
    {
        isComboing = false;
        SwitchState(EnemyState.IDLE);
    }

    // ���ܻ���������ʱ�� Animation Event ����
    public void ResetHurtStateFromAnimation()
    {
        SwitchState(EnemyState.IDLE);
    }

    #endregion

    #region ���ǻ��෽��

    // ����Hurt�����������ܻ��߼�
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

    // ����Dead���������������߼�
    protected override void Dead()
    {
        base.Dead();
        SwitchState(EnemyState.DEATH);
        StartCoroutine(DelayDead());
    }
    #endregion

    #region ����ר���߼�

    // �ӳ�����Э��
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

    // ״̬�л����������й�������״̬ת��
    public void SwitchState(EnemyState newState)
    {
        if (state == newState)
        {
            return;
        }

        // �˳���ǰ״̬�������߼�
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

        state = newState; // ���µ�ǰ״̬

        // ���ĳЩ״̬���ڽ���ʱ��Ҫ��������Animator Trigger (��Ϊ��Trigger��������Ҫÿ��Set)
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

    // �ڱ༭���л���Gizmos�����ڵ���
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