using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleport : MonoBehaviour
{
    [Header("传送设置")]
    [SerializeField] private string targetScene = "Boss1";
    [SerializeField] private bool requireSaveBeforeTeleport = true;
    [SerializeField] private float saveDelay = 0.5f; // 保存延迟时间
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            StartCoroutine(TeleportWithSave());
        }
    }
    
    private IEnumerator TeleportWithSave()
    {
        if (requireSaveBeforeTeleport)
        {
            // 确保所有管理器都已初始化
            yield return null;

            //保存HP
            CharacterData cd = FindObjectOfType<CharacterData>();
            if (cd != null)
            {
                cd.SaveCharacterData();
                Debug.Log("玩家数据已保存");
            }
            
            // 保存道德系统数据
            MoralitySystem moralitySystem = FindObjectOfType<MoralitySystem>();
            if (moralitySystem != null)
            {
                moralitySystem.SavePlayerData();
                Debug.Log("道德值数据已保存");
            }
            
            // 保存技能系统数据
            AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager != null)
            {
                abilityManager.SaveAbilities();
                Debug.Log("技能数据已保存");
            }
            
            // 等待短暂时间确保数据保存完成
            yield return new WaitForSeconds(saveDelay);
        }
        
        // 加载目标场景
        SceneManager.LoadScene(targetScene);
    }
}