using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    [Header("所有技能")]
    [SerializeField] private MoralitySystem moralitySystem;
    public List<Ability> allAbilities = new List<Ability>();
    
    // 技能名称常量
    private const string SWORD = "00_Sword";
    private const string SCROLL = "01_Scroll";
    private const string FIRE = "02_Fire";
    private const string THUNDER = "03_Thunder";
    private const string STEPS = "04_Steps";
    private const string QI_BLOOD = "05_Qi-Blood";
    private const string BIG_BOOK = "06_BigBook";
    private const string BIG_SWORD = "07_BigSword";
    private const string BLOOD_SWORD = "08_BloodSword";
    private const string WATER_PROOF = "09_WaterProof";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        // 自动查找并填充所有技能
        FindAllAbilities();
    }

    void Start()
    {
        foreach (Ability ability in allAbilities)
        {
            if (!ability.isLearned)
                DarkenAbilityImage(ability.gameObject); // 未学习：变暗
        }
    }

    private void Update()
    {
        foreach (Ability ability in allAbilities)
        {
            if (ability.isLearned)
                RefreshAbility(ability.gameObject);     // 已学习：恢复
        }
    }

    // 自动查找所有技能
    private void FindAllAbilities()
    {
        // 清空现有列表
        allAbilities.Clear();
        
        // 查找所有Ability对象
        Ability[] foundAbilities = Resources.FindObjectsOfTypeAll<Ability>();
        
        // 按名称排序并添加到列表
        foreach (string abilityName in new string[] {SWORD, SCROLL, FIRE, THUNDER, STEPS, 
                    QI_BLOOD, BIG_BOOK, BIG_SWORD, BLOOD_SWORD, WATER_PROOF})
        {
            foreach (Ability ability in foundAbilities)
            {
                if (ability.name == abilityName)
                {
                    allAbilities.Add(ability);
                    break;
                }
            }
        }
    }

    // 保存所有技能状态
    public void SaveAbilities()
    {
        foreach (Ability ability in allAbilities)
        {
            PlayerPrefs.SetInt(ability.name, ability.isLearned ? 1 : 0);
        }
        PlayerPrefs.Save();
    }

    // 加载所有技能状态
    public void LoadAbilities()
    {
        foreach (Ability ability in allAbilities)
        {
            if (PlayerPrefs.HasKey(ability.name))
            {
                ability.isLearned = PlayerPrefs.GetInt(ability.name) == 1;
            }
            else
            {
                // 如果没有保存的数据，保持默认值
                ability.isLearned = false;
            }
        }
    }
    
    public int get_skil_sword_attack()
    {
        int swordAttack = 1;
        foreach (Ability ability in allAbilities)
        {
            if (ability.name == FIRE || ability.name == THUNDER || 
                ability.name == BIG_SWORD || ability.name == BLOOD_SWORD)
            {
                if (ability.isLearned)
                {
                    if (ability.name == THUNDER || ability.name == FIRE)
                    {
                        swordAttack += 1;
                    }
                    else if (ability.name == BIG_SWORD)
                        swordAttack += 5;
                    else if (ability.name == BLOOD_SWORD)
                        swordAttack += 10;
                }
            }
        }
        return swordAttack + moralitySystem.get_MaxMorality_attack();
    }
    
    public int get_skil_scroll_attack()
    {
        int scrollAttack = 1;
        foreach (Ability ability in allAbilities)
        {
            if (ability.name == BIG_BOOK && ability.isLearned)
            {
                scrollAttack += 1;
            }
        }
        return scrollAttack + moralitySystem.get_MaxMorality_attack();
    }
    
    public void SetAllButtonActiveFalse()
    {
        foreach (Ability ability in allAbilities)
        {
            // 隐藏升级按钮
            if (ability.upgradeButton != null)
                ability.upgradeButton.gameObject.SetActive(false);
        }
    }

    // 恢复已学习技能的原始颜色和状态
    private void RefreshAbility(GameObject abilityObj)
    {
        Transform abilityImageTransform = abilityObj.transform.Find("Ability Image");
        if (abilityImageTransform != null)
        {
            Image abilityImage = abilityImageTransform.GetComponent<Image>();
            if (abilityImage != null)
            {
                abilityImage.color = Color.white;
            }
            else
            {
                Debug.LogWarning($"No Image component found on 'ability Image' child of {abilityObj.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No child named 'ability Image' found in {abilityObj.name}");
        }
    }

    // 未学习技能变暗处理
    private void DarkenAbilityImage(GameObject abilityObj)
    {
        Transform abilityImageTransform = abilityObj.transform.Find("Ability Image");
        if (abilityImageTransform != null)
        {
            Image abilityImage = abilityImageTransform.GetComponent<Image>();
            if (abilityImage != null)
            {
                Color currentColor = abilityImage.color;
                print(currentColor.r+ " " + currentColor.g + " " + currentColor.b);
                Color darkenedColor = new Color(
                    Mathf.Max(0.3f, currentColor.r-0.5f),
                    Mathf.Max(0.3f, currentColor.g-0.5f),
                    Mathf.Max(0.3f, currentColor.b-0.5f),
                    currentColor.a
                );
                abilityImage.color = darkenedColor;
            }
            else
            {
                Debug.LogWarning($"No Image component found on 'ability Image' child of {abilityObj.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No child named 'ability Image' found in {abilityObj.name}");
        }
    }
}