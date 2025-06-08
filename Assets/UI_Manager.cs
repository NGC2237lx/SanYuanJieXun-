using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour
{
    [Header("界面配置")]
    [SerializeField] private GameObject skillTreePanel;  // 技能树界面
    [SerializeField] private GameObject[] otherPanels;   // 需要被隐藏的其他界面（如主菜单、背包等）

    [Header("按钮配置")]
    [SerializeField] private Button enterButton;  // 进入技能树的按钮
    [SerializeField] private Button backButton;    // 返回按钮

    private void Start()
    {
        // 绑定按钮事件
        enterButton.onClick.AddListener(ShowSkillTree);
        backButton.onClick.AddListener(HideSkillTree);
        
        // 初始化状态
        skillTreePanel.SetActive(false);
    }

    /// <summary>
    /// 显示技能树界面并隐藏其他界面
    /// </summary>
    private void ShowSkillTree()
    {
        // 隐藏所有其他界面
        foreach (var panel in otherPanels)
        {
            if (panel != null) panel.SetActive(false);
        }
        
        // 显示技能树
        skillTreePanel.SetActive(true);
    }

    /// <summary>
    /// 隐藏技能树界面并显示其他界面
    /// </summary>
    private void HideSkillTree()
    {
        // 隐藏技能树
        skillTreePanel.SetActive(false);
        
        // 显示其他界面（默认显示第一个）
        if (otherPanels.Length > 0 && otherPanels[0] != null)
        {
            otherPanels[0].SetActive(true); 
        }
    }

    private void OnDestroy()
    {
        // 移除按钮监听（防止内存泄漏）
        enterButton.onClick.RemoveListener(ShowSkillTree);
        backButton.onClick.RemoveListener(HideSkillTree);
    }
}