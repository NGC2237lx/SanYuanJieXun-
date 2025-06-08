using UnityEngine;
using UnityEngine.UI;

public class MoralitySystem : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float maxMorality = 100f;
    [SerializeField] private float currentMorality = 50f; // 初始中立值50

    [Header("Gameplay Effects")]
    [SerializeField] private float attackBonusPer10Points = 1f;
    public bool CanEnterTemple { get; private set; }
    public float AttackPowerBonus { get; private set; }

    [Header("References")]
    [SerializeField] private CharacterController2D characterController;
    [SerializeField] private RectTransform whiteBar;
    [SerializeField] private RectTransform blackBar;

    private float initialBlackBarWidth;
    private float totalWidth;
    private const float TempleThreshold = 60f;
    private float baseSlashDamage; // 存储基础攻击力

    private void Awake()
    {
        // 确保在Start之前获取所有必要引用
        FindCharacterController();
    }

    private void Start()
    {
        InitializeSystem();

        // 获取基础攻击力
        if (characterController != null)
        {
            baseSlashDamage = characterController.slashDamage;
        }
        else
        {
            Debug.LogWarning("CharacterController2D reference is missing! Attack bonuses will not be applied.");
        }

        UpdateGameplayEffects();
    }

    // 专门用于查找角色控制器的方法
    private void FindCharacterController()
    {
        if (characterController == null)
        {
            // 先尝试从父对象获取
            characterController = GetComponentInParent<CharacterController2D>();

            // 如果还是null，尝试在整个场景中查找
            if (characterController == null)
            {
                characterController = FindObjectOfType<CharacterController2D>();

                if (characterController != null)
                {
                    Debug.Log("Found CharacterController2D in scene, but recommend assigning it directly in inspector.");
                }
            }
        }
    }

    private void InitializeSystem()
    {
        initialBlackBarWidth = blackBar.sizeDelta.x;
        totalWidth = initialBlackBarWidth * 2;
        UpdateBars();
    }

    private void UpdateBars()
    {
        float moralityRatio = currentMorality / maxMorality;
        float whiteWidth = totalWidth * moralityRatio;
        float blackWidth = totalWidth - whiteWidth;

        whiteBar.sizeDelta = new Vector2(whiteWidth, whiteBar.sizeDelta.y);
        blackBar.sizeDelta = new Vector2(blackWidth, blackBar.sizeDelta.y);
    }

    public void ChangeMorality(float amount)
    {
        float oldValue = currentMorality;
        currentMorality = Mathf.Clamp(currentMorality + amount, 0, maxMorality);

        UpdateBars();

        if (!Mathf.Approximately(oldValue, currentMorality))
        {
            UpdateGameplayEffects();
        }
    }

    private void UpdateGameplayEffects()
    {
        // 更新城隍庙进入权限
        CanEnterTemple = currentMorality >= TempleThreshold;

        // 计算攻击加成 (基于与中立值50的差值)
        float moralityDifference = 50f - currentMorality;

        // 只有差值≥0时才计算加成（低于0不计）
        AttackPowerBonus = moralityDifference >= 0 ?
            Mathf.Floor(moralityDifference / 10f) * attackBonusPer10Points : 0f;

        // 更新角色攻击力 (四舍五入为整数)
        if (characterController != null)
        {
            characterController.slashDamage = Mathf.RoundToInt(baseSlashDamage + AttackPowerBonus);
        }

        Debug.Log($"攻击力更新: 基础{baseSlashDamage} + 加成{AttackPowerBonus} = {baseSlashDamage + AttackPowerBonus}");
    }

    // 编辑器按钮，用于测试查找功能
    [ContextMenu("Try Find Character Controller")]
    private void EditorTryFindController()
    {
        FindCharacterController();
        if (characterController != null)
        {
            Debug.Log($"Found controller: {characterController.gameObject.name}");
        }
        else
        {
            Debug.LogWarning("No CharacterController2D found!");
        }
    }

    public int get_MaxMorality_attack()
    {
        // 计算攻击加成 (基于与中立值50的差值)
        float moralityDifference = 50f - currentMorality;

        // 只有差值≥0时才计算加成（低于0不计）
        AttackPowerBonus = moralityDifference >= 0 ?
            Mathf.Floor(moralityDifference / 10f) * attackBonusPer10Points : 0f;
        return Mathf.RoundToInt(AttackPowerBonus);
    }

    // 保存玩家数据到PlayerPrefs
    private const string MoralityKey = "Dao";
    public void SavePlayerData()
    {
        PlayerPrefs.SetFloat(MoralityKey, currentMorality);
        Debug.Log($"保存道德值: {currentMorality}");
        PlayerPrefs.Save(); // 立即保存到磁盘
    }

    // 从PlayerPrefs加载玩家数据
    public void LoadDao()
    {
        // 如果存在保存的道德值，则加载它
        if (PlayerPrefs.HasKey(MoralityKey))
        {
            currentMorality = PlayerPrefs.GetFloat(MoralityKey);
            ChangeMorality(0); // 更新UI和效果
            Debug.Log($"加载道德值: {currentMorality}");
        }
        else
        {
            Debug.Log("没有找到保存的道德值，使用默认值");
        }
    }

}