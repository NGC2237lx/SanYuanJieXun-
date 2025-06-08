using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class MySceneLoadManager : MonoBehaviour
{
    [Header("加载设置")]
    [SerializeField] private float loadDelay = 0.5f; // 延迟加载时间
    
    // 当脚本被加载时调用
    private void Awake()
    {
        // 确保只有一个实例存在
        if (FindObjectsOfType<MySceneLoadManager>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(InitializeSceneData());
    }

    private IEnumerator InitializeSceneData()
    {
        // 等待一帧确保所有对象都已初始化
        yield return null;

        // 加载玩家数据
        CharacterData characterData = FindObjectOfType<CharacterData>();
        if (characterData != null)
        {
            characterData.LoadCharacterData();
            Debug.Log($"场景 {SceneManager.GetActiveScene().name} 玩家数据已加载");
        }
        // 加载道德系统数据
        MoralitySystem moralitySystem = FindObjectOfType<MoralitySystem>();
        if (moralitySystem != null)
        {
            moralitySystem.LoadDao();
            Debug.Log($"场景 {SceneManager.GetActiveScene().name} 道德值数据已加载");
        }
        AbilityManager abilityManager = FindObjectOfType<AbilityManager>();
        if (abilityManager != null)
        {
            abilityManager.LoadAbilities();
            Debug.Log($"场景 {SceneManager.GetActiveScene().name} 技能数据已加载");
        }
        FireMoneyCollector fc = FindObjectOfType<FireMoneyCollector>();
        if (fc != null)
        {
            fc.LoadFireMoneyCount();
            Debug.Log($"场景 {SceneManager.GetActiveScene().name} 香火钱数据已加载");
        }

        // 等待短暂时间确保其他系统初始化完成
        yield return new WaitForSeconds(loadDelay);
        

        
    }

}