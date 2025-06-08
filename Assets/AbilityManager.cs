using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityManager : MonoBehaviour
{
    public static AbilityManager Instance;

    [Header("所有技能")]

    [SerializeField] private MoralitySystem moralitySystem;
    public List<Ability> allAbilities = new List<Ability>();
    

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
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
            if (ability.name == "02_Fire" || ability.name == "03_Thunder" || ability.name == "07_BigSword" || ability.name == "08_BloodSword")
            {


                if (ability.isLearned)
                {
                    if (ability.name == "03_Thunder" || ability.name == "02_Fire")
                    {
                        swordAttack += 1;
                    }
                    else if (ability.name == "07_BigSword")
                        swordAttack += 5;
                    else if (ability.name == "08_BloodSword")
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
            if (ability.name == "06_BigBook")
            {
                if (ability.isLearned)
                {
                    scrollAttack += 1;
                }
            }
        }
        return scrollAttack+moralitySystem.get_MaxMorality_attack();
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
                // 恢复为白色（原始颜色）
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
                //print(currentColor.r + " " + currentColor.g + " " + currentColor.b);
                Color darkenedColor = new Color(
                    Mathf.Max(0, currentColor.r-0.7f),
                    Mathf.Max(0, currentColor.g-0.7f),
                    Mathf.Max(0, currentColor.b-0.7f),
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