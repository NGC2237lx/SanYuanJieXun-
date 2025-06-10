using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    // UI元素名称常量
    private const string SKILL_TREE_PANEL = "SkillTreeCanvas";
    private const string ENTER_BUTTON = "EnterButton";
    private const string BACK_BUTTON = "BackButton";
    
    // 自动查找的UI元素
    private GameObject skillTreePanel;
    private Button enterButton;
    private Button backButton;

    private void Start()
    {
        // 自动查找UI元素
        skillTreePanel = GameObject.Find(SKILL_TREE_PANEL);
        enterButton = GameObject.Find(ENTER_BUTTON)?.GetComponent<Button>();
        backButton = GameObject.Find(BACK_BUTTON)?.GetComponent<Button>();

        // 验证查找结果
        if (skillTreePanel == null) Debug.LogError($"找不到技能树面板: {SKILL_TREE_PANEL}");
        if (enterButton == null) Debug.LogError($"找不到进入按钮: {ENTER_BUTTON}");
        if (backButton == null) Debug.LogError($"找不到返回按钮: {BACK_BUTTON}");

        // 绑定按钮事件
        if (enterButton != null) enterButton.onClick.AddListener(ShowSkillTree);
        if (backButton != null) backButton.onClick.AddListener(HideSkillTree);
        
        // 初始隐藏技能树
        if (skillTreePanel != null) skillTreePanel.SetActive(false);
    }

    /// 显示技能树界面
    private void ShowSkillTree()
    {
        if (skillTreePanel != null)
        {
            skillTreePanel.SetActive(true);
            AbilityManager.Instance?.SetAllButtonActiveFalse(); // 刷新技能按钮状态
        }
    }

    /// 隐藏技能树界面
    private void HideSkillTree()
    {
        if (skillTreePanel != null) skillTreePanel.SetActive(false);
    }

    private void OnDestroy()
    {
        // 安全移除事件监听
        if (enterButton != null) enterButton.onClick.RemoveListener(ShowSkillTree);
        if (backButton != null) backButton.onClick.RemoveListener(HideSkillTree);
    }
}