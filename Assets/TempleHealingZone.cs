using UnityEngine;

public class TempleHealingZone : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private LayerMask heroLayer; // 玩家图层
    [SerializeField] private float healCooldown = 5f; // 治疗冷却时间
    [SerializeField] private bool requireGoodMorality = true; // 是否需要道德检查

    [Header("References")]
    [SerializeField] private MoralitySystem moralitySystem; // 道德系统引用

    [Header("Effects")]
    [SerializeField] private ParticleSystem healingEffect;
    [SerializeField] private AudioClip healingSound;

    private float lastHealTime;
    private AudioSource audioSource;

    private void Awake()
    {
        // 确保有AudioSource组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 尝试自动获取道德系统
        if (moralitySystem == null)
        {
            moralitySystem = FindObjectOfType<MoralitySystem>();
            if (moralitySystem == null)
            {
                Debug.LogWarning("未找到MoralitySystem组件！治疗将不考虑道德值");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家图层且满足治疗条件
        if (other.gameObject.layer == LayerMask.NameToLayer("Hero Detector") && CanHeal())
        {
            TryHealPlayer(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // 持续检测，避免错过触发
        if (other.gameObject.layer == LayerMask.NameToLayer("Hero Detector") && CanHeal())
        {
            TryHealPlayer(other);
        }
    }

    private bool CanHeal()
    {
        // 检查冷却时间
        bool canHeal = Time.time > lastHealTime + healCooldown;
        
        // 如果需要道德检查且道德系统存在
        if (requireGoodMorality && moralitySystem != null)
        {
            canHeal &= moralitySystem.CanEnterTemple;
            
            // 调试信息
            if (!moralitySystem.CanEnterTemple)
            {
                Debug.Log("道德值不足，无法在城隍庙治疗");
            }
        }
        
        return canHeal;
    }

    private void TryHealPlayer(Collider2D playerCollider)
    {
        CharacterData playerHealth = playerCollider.GetComponentInParent<CharacterData>();
        if (playerHealth != null)
        {
            // 调用治疗
            playerHealth.HealAtTemple();
            lastHealTime = Time.time;
            
            // 播放效果
            PlayHealingEffects();
            
            Debug.Log("玩家在城隍庙接受治疗");
        }
    }

    private void PlayHealingEffects()
    {
        // 播放粒子效果
        if (healingEffect != null)
        {
            healingEffect.Play();
        }
        
        // 播放音效
        if (healingSound != null)
        {
            audioSource.PlayOneShot(healingSound);
        }
    }

    // 可视化冷却时间和道德状态
    private void OnDrawGizmosSelected()
    {
        if (Time.time < lastHealTime + healCooldown)
        {
            Gizmos.color = Color.red; // 冷却中
        }
        else if (requireGoodMorality && moralitySystem != null && !moralitySystem.CanEnterTemple)
        {
            Gizmos.color = Color.yellow; // 道德不足
        }
        else
        {
            Gizmos.color = Color.green; // 可治疗
        }
        
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }

    // 编辑器按钮，用于测试道德检查
    [ContextMenu("测试道德检查")]
    private void TestMoralityCheck()
    {
        if (moralitySystem != null)
        {
            Debug.Log($"当前可进入城隍庙状态: {moralitySystem.CanEnterTemple}");
        }
        else
        {
            Debug.LogWarning("未找到MoralitySystem引用");
        }
    }
}