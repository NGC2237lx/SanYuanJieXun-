using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileDamage : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask enemyLayer; // 敌人图层
    [SerializeField] private int baseDamage = 1; // 基础伤害
    [SerializeField] private bool destroyOnHit = true; // 击中后是否销毁


    [Header("References")]
    [SerializeField] private CharacterController2D characterController; // 主角控制器

    [SerializeField] private AbilityManager abilityManager;
    private void Awake()
    {
        // 自动初始化敌人图层
        enemyLayer = LayerMask.NameToLayer("Enemy Detector");

        // 尝试自动获取主角控制器
        if (characterController == null)
        {
            characterController = FindObjectOfType<CharacterController2D>();
        }

        abilityManager = FindObjectOfType<AbilityManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是敌人图层
        if (other.gameObject.layer != enemyLayer) return;

        // 获取敌人的Hurt方法
        Enemy enemy = other.GetComponentInParent<Enemy>();
        if (enemy != null)
        {
            // 计算最终伤害（基础伤害 + 主角slashDamage）
            int finalDamage = abilityManager.get_skil_scroll_attack();
            
            // 调用伤害方法
            enemy.Hurt(finalDamage);
            
            Debug.Log($"符咒造成伤害: {finalDamage} ");

            // 击中后销毁
            if (destroyOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    // 编辑器按钮，用于测试图层设置
    [ContextMenu("Verify Enemy Layer")]
    private void VerifyEnemyLayer()
    {
        if (enemyLayer == LayerMask.NameToLayer("Enemy Detector"))
        {
            Debug.Log("敌人图层设置正确");
        }
        else
        {
            Debug.LogWarning($"敌人图层设置不正确，当前: {LayerMask.LayerToName(enemyLayer)}");
        }
    }
}