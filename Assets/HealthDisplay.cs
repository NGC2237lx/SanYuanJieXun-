using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class HealthDisplayTMP : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterData characterData;
    [SerializeField] private TextMeshProUGUI healthText;
    
    [SerializeField] private Image healthBarImage; 

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
        int maxHealth = characterData.GetMaxHealth();
        healthText.text = $"{currentHealth}/{maxHealth}";
        
        float healthPercent = (float)currentHealth / maxHealth;
        RectTransform rt = healthBarImage.rectTransform;
        rt.sizeDelta = new Vector2(200 * healthPercent, rt.sizeDelta.y);
    }
}