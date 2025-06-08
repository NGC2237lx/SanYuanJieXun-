using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Ability : MonoBehaviour
{
    [Header("基础信息")]
    public string abilityName;
    public Sprite abilitySprite;
    [TextArea(1, 3)] public string abilityDes;
    public int upgradeCost = 100; // 升级所需香火钱

    [Header("技能依赖")]
    public bool isUpgrade; // 是否为进阶技能
    public Ability[] previousAbility; // 前置技能

    [Header("UI引用")]
    public DisplayPanel displayPanel;
    public Button upgradeButton; // 独立的升级按钮
    public GameObject notEnoughText; // 香火钱不足提示文本

    [Header("状态")]
    public bool isLearned;
    public FireMoneyCollector moneySystem;

    public AbilityManager abilityManager;

    private void Start()
    {
        // 绑定查看技能按钮
        GetComponent<Button>().onClick.AddListener(ShowAbilityInfo);

        // 绑定升级按钮
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(TryUpgrade);
            upgradeButton.gameObject.SetActive(false); // 默认隐藏
        }

        // 确保提示文本初始隐藏
        if (notEnoughText != null)
        {
            notEnoughText.SetActive(false);
        }
    }

    // 显示技能信息（无论是否学习）
    public void ShowAbilityInfo()
    {
        abilityManager.SetAllButtonActiveFalse();

        if(IsUnlockable() && !isLearned) upgradeButton.gameObject.SetActive(true);

        if (displayPanel != null)
        {
            displayPanel.Show(abilityName, abilitySprite, $"{abilityDes}\n\n消耗香火钱：{upgradeCost}");
        }
    }

    // 尝试升级技能
    private void TryUpgrade()
    {
        if (moneySystem == null) return;

        // 检查香火钱是否足够
        if (moneySystem.GetFireMoneyCount() >= upgradeCost)
        {
            moneySystem.ReduceFireMoney(upgradeCost);
            LearnAbility();
            upgradeButton.gameObject.SetActive(false);
        }
        else
        {
            // 显示香火钱不足提示
            ShowNotEnoughText();
        }
    }

    // 显示香火钱不足提示
    private void ShowNotEnoughText()
    {
        if (notEnoughText != null)
        {
            // 激活文本
            notEnoughText.SetActive(true);
            // 1秒后隐藏
            StartCoroutine(HideNotEnoughTextAfterDelay(1f));
        }
    }

    // 延迟隐藏文本的协程
    private IEnumerator HideNotEnoughTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        notEnoughText.SetActive(false);
    }

    // 学习技能
    private void LearnAbility()
    {
        if (name == "05_Qi-Blood")
        {
            CharacterData cd = FindObjectOfType<CharacterData>();
            cd.SetMaxHealth(10);
        }
        Debug.Log($"学习技能: {abilityName} (对象: {gameObject.name})");
        isLearned = true;
    }

    // 检查是否可解锁
    private bool IsUnlockable()
    {
        // 基础技能无需前置
        if (!isUpgrade) return true;

        // 检查所有前置技能
        foreach (Ability reqAbility in previousAbility)
        {
            if (reqAbility != null && !reqAbility.isLearned)
            {
                return false;
            }
        }
        return true;
    }
}