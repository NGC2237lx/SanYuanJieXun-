using UnityEngine;

public class CorpseInteractor : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask enemyDetectorLayer;
    [SerializeField] private int redeemValue = 1;    // 超度增加的善值
    [SerializeField] private int consumeValue = -1; // 吞噬减少的善值
    
    [Header("References")]
    [SerializeField] private MoralitySystem moralitySystem;
    
    private Transform currentCorpse;

    private void Update()
    {
        if (currentCorpse != null)
        {
            // 按1键超度
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                RedeemCorpse();
            }
            // 按2键吞噬
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                ConsumeCorpse();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy Detector"))
        {
            Enemy enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null && enemy.IsDeadOrNot())
            {
                currentCorpse = enemy.transform;
                Debug.Log("发现尸体 - 按1超度(+善) / 按2吞噬(+恶)");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (currentCorpse != null && other.transform == currentCorpse)
        {
            currentCorpse = null;
            Debug.Log("离开尸体范围");
        }
    }

    private void RedeemCorpse()
    {
        if (moralitySystem != null)
        {
            moralitySystem.ChangeMorality(redeemValue);
            Debug.Log($"超度尸体，善值+{redeemValue}");
        }
        DestroyCorpse();
    }

    private void ConsumeCorpse()
    {
        if (moralitySystem != null)
        {
            moralitySystem.ChangeMorality(consumeValue);
            Debug.Log($"吞噬尸体，恶值{consumeValue}");
        }
        DestroyCorpse();
    }

    private void DestroyCorpse()
    {
        if (currentCorpse != null)
        {
            Destroy(currentCorpse.gameObject);
            currentCorpse = null;
        }
    }

    // 自动获取引用（如果未手动设置）
    private void Awake()
    {
        if (moralitySystem == null)
        {
            moralitySystem = FindObjectOfType<MoralitySystem>();
        }
    }
}