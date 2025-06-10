using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine.SceneManagement;

public class start_drama : MonoBehaviour
{
    // Start is called before the first frame update
    string fullText;
    [SerializeField] 
    public TMP_Text drama;
    public TextAsset text;    
    public float charDelay = 0.05f; // 字符显示间隔
    public float punctuationDelay = 0.2f; // 标点符号额外延迟
    private bool isTyping = false;
    public  bool isFinished = false;

    void Start()
    {
        fullText = text.text;
        isTyping = true;
        StartCoroutine(TypeText());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isTyping)
        {
            isFinished = true;
            isTyping = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isFinished)
        {
            GameStart();
        }
        
    }

    IEnumerator TypeText()
    {
        drama.text = "";
        foreach (char c in fullText)
        {
            drama.text += c;
            if (isFinished)
            {
                drama.text = fullText;
                break;
            }                
            // 中文标点符号额外延迟
            if (IsChinesePunctuation(c))
                yield return new WaitForSeconds(punctuationDelay);
            else
                yield return new WaitForSeconds(charDelay);
        }
        isFinished = true;
        isTyping = false;
    }

    bool IsChinesePunctuation(char c)
    {
        // 常见中文标点
        char[] punctuations = { '，', '。', '！', '？', '：', '；', '“', '”', '（', '）' };
        return System.Array.IndexOf(punctuations, c) >= 0;
    }

    void GameStart()
    {
        print("New Scene");
        // 游戏开始逻辑
        SceneManager.LoadScene("关卡1");
    }
}
