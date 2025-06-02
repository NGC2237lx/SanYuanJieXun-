using UnityEngine;
using System.Collections; // 添加此命名空间解决IEnumerator错误
public class PlayerShooting : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject projectilePrefab; // 子弹预制体
    [SerializeField] private float projectileSpeed = 10f; // 子弹速度
    [SerializeField] private float projectileLifetime = 5f; // 子弹存活时间
    [SerializeField] private float fireCooldown = 0.5f; // 发射冷却时间

    [Header("References")]
    [SerializeField] private Transform firePoint; // 发射点

    [Header("Animation")]
    [SerializeField] private Animator playerAnimator; // 角色动画控制器
    [SerializeField] private string shootTrigger = "Shoot"; // 动画触发器名称

    private float lastFireTime;
    private bool facingRight = true; // 默认朝右
    private bool isShooting = false; // 新增射击状态标志
    private void Awake()
    {
        // 确保发射点已设置
        if (firePoint == null)
        {
            Debug.LogError("FirePoint未设置！");
        }
    }

    private void Update()
    {
        // 检测角色朝向（可根据实际项目调整）
        float moveInput = Input.GetAxis("Horizontal");
        if (moveInput > 0)
        {
            facingRight = true;
        }
        else if (moveInput < 0)
        {
            facingRight = false;
        }

        // 检测发射输入（C键）
        if (Input.GetKeyDown(KeyCode.C) && Time.time > lastFireTime + fireCooldown)
        {
            isShooting = true; // 标记开始射击
            Shoot();
            // 触发射击动画
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger(shootTrigger);
                StartCoroutine(ResetShootTrigger());
            }
            else
            {
                isShooting = false; // 标记停止射击
            }
            
        }
    }

    private void Shoot()
    {
        // 确定发射方向（水平方向）
        Vector2 shootDirection = facingRight ? Vector2.right : Vector2.left;

        // 实例化子弹
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        
        // 设置子弹速度和方向
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = shootDirection * projectileSpeed;
        }

        // 设置子弹旋转朝向移动方向
        float angle = facingRight ? 0f : 180f;
        projectile.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // 添加自动销毁组件
        AutoDestroyAfterTime destroyer = projectile.AddComponent<AutoDestroyAfterTime>();
        destroyer.lifetime = projectileLifetime;

        // 记录最后发射时间
        lastFireTime = Time.time;
    }
    // 新增协程方法
    private IEnumerator ResetShootTrigger()
    {
        // 等待动画进入Shoot状态
        yield return new WaitForEndOfFrame();

        // 获取动画长度
        float animLength = playerAnimator.GetCurrentAnimatorStateInfo(0).length;

        // 等待动画播放完成
        yield return new WaitForSeconds(animLength);

        // 重置状态和触发器
        isShooting = false;
        playerAnimator.ResetTrigger(shootTrigger);
    }
}

// 自动销毁组件
public class AutoDestroyAfterTime : MonoBehaviour
{
    public float lifetime = 5f;
    private float spawnTime;

    private void Start()
    {
        spawnTime = Time.time;
    }

    private void Update()
    {
        if (Time.time > spawnTime + lifetime)
        {
            Destroy(gameObject);
        }
    }
}