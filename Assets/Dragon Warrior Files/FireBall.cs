using System.Collections;
using UnityEngine;
public class Fireball : MonoBehaviour
{
    [Header("Settings")]
    public int damage = 25;
    public float speed = 12f;
    public LayerMask playerLayer;
    public float lifetime = 3f;

    private Vector2 direction;
    private Collider2D ballCollider;
    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 0;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        // ��ʼ����ײ���ӳ����ã���ֹ���ˣ�
        ballCollider = GetComponent<Collider2D>();
        if (ballCollider != null)
        {
            ballCollider.enabled = false;
            StartCoroutine(EnableColliderAfter(0.1f));
        }

        // ȷ����������
        Destroy(gameObject, lifetime);
    }

    IEnumerator EnableColliderAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ballCollider != null)
            ballCollider.enabled = true;
    }

    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized;

        // ���ø����ٶ�
        if (rb != null)
        {
            rb.velocity = direction * speed;
        }

        // ���ݷ�����ת����
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // �����뷢���ߵ���ײ
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            return;

        // ��Ҽ��
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            print("Fireball hit player: " + other.gameObject.name);
            CharacterController2D player = other.GetComponent<CharacterController2D>();
            if (player != null)
            {
                CharacterData playerData = player.GetComponent<CharacterData>();
                if (playerData != null && !playerData.GetDeadStatement())
                {
                    StartCoroutine(player.TakeDamage());
                }
                DestroyFireball();
            }
            else print("No CharacterController2D found on player: " + other.gameObject.name);
            
        }
        // ���μ��
        else if (other.gameObject.layer == LayerMask.NameToLayer("Terrain"))
        {
            print("Fireball hit terrain"+ other.gameObject.name);
            DestroyFireball();
        }
    }

    private void DestroyFireball()
    {
        // ��ӱ�ըЧ��������У�
        //if (TryGetComponent(out FireballEffect effect))
        //{
        //    effect.PlayExplosion();
        //}
        Destroy(gameObject);
    }
}