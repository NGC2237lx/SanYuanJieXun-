using UnityEngine;
using UnityEngine.SceneManagement;

public class DontDestroyOnLoad : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    public string stopSceneName1="大Boss"; // 在Inspector中设置需要停止的关卡名称
    public string stopSceneName2="PlayIntro"; // 在Inspector中设置需要停止的关卡名称
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == stopSceneName1|| scene.name == stopSceneName2)
        {
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
                Destroy(gameObject); // 停止后销毁对象
            }
        }
    }
}