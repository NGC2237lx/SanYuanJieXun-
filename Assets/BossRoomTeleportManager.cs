using UnityEngine;

public class BossTeleportController : MonoBehaviour
{
    [Header("Boss设置")]
    [SerializeField] private Enemy bossEnemy;  // 拖入Boss的Enemy组件
    [SerializeField] private Vector3 spawnOffset = Vector3.zero; // 生成位置偏移

    [Header("传送门组件")]
    [SerializeField] private GameObject teleportVisuals; // 包含渲染器和碰撞体的子物体
    [SerializeField] private Collider2D teleportCollider;

    private void Start()
    {
        // 初始隐藏传送门
        if (teleportVisuals != null) teleportVisuals.SetActive(false);
        if (teleportCollider != null) teleportCollider.enabled = false;
    }

    private void Update()
    {
        // 每帧检查Boss状态
        if (bossEnemy != null && bossEnemy.IsDeadOrNot())
        {
            ActivateTeleport();
            enabled = false; // 激活后禁用此脚本
        }
    }

    private void ActivateTeleport()
    {
        // 设置传送门位置（Boss死亡位置+偏移）
        teleportVisuals.transform.position = new Vector3(bossEnemy.transform.position.x , 
                                              bossEnemy.transform.position.y , 
                                              -1f);
        
        // 激活传送门
        if (teleportVisuals != null) teleportVisuals.SetActive(true);
        if (teleportCollider != null) teleportCollider.enabled = true;
        
        Debug.Log("传送门已激活在Boss死亡位置");
    }
}