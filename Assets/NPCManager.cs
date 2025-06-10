using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCManager : MonoBehaviour
{
    [Header("对话设置")]
    public string canvasName = "DialogueCanvas"; // 画布名称
    public string npcPortraitName = "NPC_Portrait"; // NPC头像名称
    public string playerPortraitName = "Player_Portrait"; // 玩家头像名称
    public string dialogueTextName = "Dialogue_Text"; // 对话文本名称

    private GameObject dialogueCanvas;
    private Image npcPortrait;
    private Image playerPortrait;
    private TMP_Text dialogueText;

    [Header("对话内容")]
    [TextArea(3, 10)]
    public string[] dialogueLines;   // 对话内容数组

    private bool isPlayerInRange = false;
    private int currentLine = 0;
    private bool isDialogueActive = false;

    private void Awake()
    {
        // 自动查找UI组件
        dialogueCanvas = GameObject.Find(canvasName);
        
        if (dialogueCanvas != null)
        {
            npcPortrait = dialogueCanvas.transform.Find(npcPortraitName)?.GetComponent<Image>();
            playerPortrait = dialogueCanvas.transform.Find(playerPortraitName)?.GetComponent<Image>();
            dialogueText = dialogueCanvas.transform.Find(dialogueTextName)?.GetComponent<TMP_Text>();
            Debug.Log($"找到画布: {dialogueCanvas.name}");
            if (npcPortrait == null || playerPortrait == null || dialogueText == null)
            {
                Debug.LogError("未能找到所有必需的UI组件，请检查对象名称是否正确");
            }
            
            dialogueCanvas.SetActive(false);
        }
        else
        {
            Debug.LogError($"未找到名为 {canvasName} 的画布对象");
        }
    }

    private void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E) && !isDialogueActive)
        {
            StartDialogue();
        }

        if (isDialogueActive && Input.GetKeyDown(KeyCode.Space))
        {
            NextDialogueLine();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            EndDialogue();
        }
    }

    private void StartDialogue()
    {
        currentLine = 0;
        isDialogueActive = true;
        dialogueCanvas.SetActive(true);
        UpdateDialogueUI();
    }

    private void NextDialogueLine()
    {
        currentLine++;
        
        if (currentLine >= dialogueLines.Length)
        {
            EndDialogue();
            return;
        }
        
        UpdateDialogueUI();
    }

    private void UpdateDialogueUI()
    {
        dialogueText.text = dialogueLines[currentLine];
        
        // 奇数行是NPC说话(0,2,4...)，偶数行是玩家说话(1,3,5...)
        bool isNPCTalking = currentLine % 2 == 0;
        
        npcPortrait.color = isNPCTalking ? Color.white : Color.gray;
        playerPortrait.color = isNPCTalking ? Color.gray : Color.white;
    }

    private void EndDialogue()
    {
        isDialogueActive = false;
        dialogueCanvas.SetActive(false);
        currentLine = 0;
    }
}