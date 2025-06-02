using TMPro;
using UnityEngine;

public class AttackPowerDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController2D characterController;
    [SerializeField] private MoralitySystem moralitySystem;
    [SerializeField] private TextMeshProUGUI attackPowerText;
    
    [Header("Display Settings")]
    [SerializeField] private string prefix = "Damage: ";
    [SerializeField] private string suffix = "";
    [SerializeField] private float updateInterval = 0.2f; // 更新间隔(秒)

    private float timer;
    private float lastDisplayedValue = -1f;

    private void Start()
    {
        // 确保所有引用有效
        VerifyReferences();
        
        // 立即更新一次显示
        UpdateDisplay();
    }

    private void Update()
    {
        // 按间隔更新显示，避免每帧更新
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (characterController == null) return;
        
        float currentAttackPower = characterController.slashDamage;
        
        // 只有当值变化时才更新文本
        if (!Mathf.Approximately(currentAttackPower, lastDisplayedValue))
        {
            attackPowerText.text = $"{prefix}{currentAttackPower:F1}{suffix}";
            lastDisplayedValue = currentAttackPower;
        }
    }

    private void VerifyReferences()
    {
        // 尝试自动获取缺失的引用
        if (characterController == null)
        {
            characterController = FindObjectOfType<CharacterController2D>();
        }
        
        if (moralitySystem == null)
        {
            moralitySystem = FindObjectOfType<MoralitySystem>();
        }
        
        if (attackPowerText == null)
        {
            attackPowerText = GetComponent<TextMeshProUGUI>();
        }
        
        // 如果仍然缺失，报错
        if (characterController == null)
        {
            Debug.LogError("CharacterController2D reference is missing!", this);
        }
        
        if (attackPowerText == null)
        {
            Debug.LogError("TextMeshProUGUI component is missing!", this);
        }
    }

    // 当攻击力变化时调用此方法（可由事件触发）
    public void OnAttackPowerChanged(float newValue)
    {
        attackPowerText.text = $"{prefix}{newValue:F1}{suffix}";
        lastDisplayedValue = newValue;
    }
}