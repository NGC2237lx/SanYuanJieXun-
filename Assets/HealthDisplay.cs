using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HealthDisplayTMP : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterData characterData;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private int lowHealthThreshold = 3;
    
    private void Awake()
    {
        // 自动获取引用
        if (healthText == null)
            healthText = GetComponent<TextMeshProUGUI>();
        
        if (characterData == null)
            characterData = FindObjectOfType<CharacterData>();
    }
    
    private void OnEnable()
    {
        UpdateHealthDisplay();
    }
    
    private void Update()
    {
        UpdateHealthDisplay();
    }
    
    private void UpdateHealthDisplay()
    {
        if (characterData == null || healthText == null) return;
        
        int currentHealth = characterData.GetCurrentHealth();
        healthText.text = $"HP: {currentHealth}";
        
        // 根据血量改变颜色
        healthText.color = currentHealth <= lowHealthThreshold ? lowColor : normalColor;
        
        // 可选：添加闪烁效果当血量低时
        if (currentHealth <= lowHealthThreshold)
        {
            healthText.color = Color.Lerp(lowColor, Color.yellow, Mathf.PingPong(Time.time, 1f));
        }
    }
}