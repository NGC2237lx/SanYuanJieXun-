using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class TempleDetection : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string templeTag = "ChenghuangTemple";
    [SerializeField] private float healCooldown = 5f;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem healEffect;
    [SerializeField] private AudioClip healSound;

    private CharacterData characterData;
    private float lastHealTime;
    private AudioSource audioSource;

    private void Awake()
    {
        characterData = GetComponentInParent<CharacterData>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(templeTag) && CanHeal())
        {
            PerformHeal();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag(templeTag) && CanHeal())
        {
            PerformHeal();
        }
    }

    private bool CanHeal()
    {
        return characterData != null && 
               !characterData.GetDeadStatement() &&
               Time.time > lastHealTime + healCooldown;
    }

    private void PerformHeal()
    {
        characterData.HealAtTemple();
        lastHealTime = Time.time;
        
        // 播放效果
        if (healEffect != null)
        {
            healEffect.Play();
        }
        
        if (healSound != null)
        {
            audioSource.PlayOneShot(healSound);
        }
        
        Debug.Log("在城隍庙接受治疗");
    }

    // 可视化冷却时间
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = CanHeal() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}