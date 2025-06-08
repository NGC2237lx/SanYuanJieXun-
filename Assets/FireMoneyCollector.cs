using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FireMoneyCollector : MonoBehaviour
{
    [Header("收集效果设置")]
    [SerializeField] Animator collectEffecter; // 收集时的动画效果
    [SerializeField] AudioClip[] fireMoneyCollects; // 收集音效数组
    [SerializeField] bool needToReset; // 是否需要重置计数

    [Header("UI设置")]
    [SerializeField] TextMeshProUGUI fireMoneyText; // 显示FireMoney数量的TextMeshPro
    [SerializeField] Text fireMoneyText2; // 显示FireMoney数量

    private AudioSource audioSource;
    private int fireMoneyCount = 0;
    private int animationCollectTrigger = Animator.StringToHash("Collect");

    private bool needtokaigua = true;

    private void Kaigua()
    {
        PlayerPrefs.SetInt("FireMoney", 500);
    }
    public void ResetMoney()
    {
        fireMoneyCount = Mathf.RoundToInt(PlayerPrefs.GetInt("FireMoney") * 0.7f);
        PlayerPrefs.SetInt("FireMoney", fireMoneyCount);
        UpdateFireMoneyText();
    }
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    private void Start()
    {
        // 如果需要重置，清空PlayerPrefs中的记录
        if (needToReset)
        {
            PlayerPrefs.SetInt("FireMoney", 0);
            PlayerPrefs.Save();
        }
        if (needtokaigua)
        {
            Kaigua();
        }
        // 从PlayerPrefs加载FireMoney数量
        fireMoneyCount = PlayerPrefs.GetInt("FireMoney");
        UpdateFireMoneyText();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测碰撞物体是否是FireMoney (Layer设置为"FireMoney")
        if (collision.gameObject.layer == LayerMask.NameToLayer("Geo"))
        {
            // 播放收集效果
            PlayCollectEffects();

            // 增加FireMoney数量
            fireMoneyCount++;

            // 保存到PlayerPrefs
            PlayerPrefs.SetInt("FireMoney", fireMoneyCount);
            PlayerPrefs.Save();

            // 更新UI文本
            UpdateFireMoneyText();

            // 销毁收集的FireMoney物体
            Destroy(collision.gameObject);
        }
    }

    private void PlayCollectEffects()
    {
        // 触发收集动画
        if (collectEffecter != null)
        {
            collectEffecter.SetTrigger(animationCollectTrigger);
        }

        // 播放随机收集音效
        if (fireMoneyCollects.Length > 0)
        {
            int index = Random.Range(0, fireMoneyCollects.Length);
            audioSource.PlayOneShot(fireMoneyCollects[index]);
        }
    }

    private void UpdateFireMoneyText()
    {
        if (fireMoneyText != null)
        {
            fireMoneyText.SetText("FireMoney: " + fireMoneyCount.ToString());
        }
        if (fireMoneyText2 != null)
        {
            fireMoneyText2.text = fireMoneyCount.ToString();
        }
    }

    // 外部调用的方法，用于增加FireMoney数量
    public void AddFireMoney(int amount)
    {
        fireMoneyCount += amount;
        PlayerPrefs.SetInt("FireMoney", fireMoneyCount);
        PlayerPrefs.Save();
        UpdateFireMoneyText();
    }
    public void ReduceFireMoney(int amount)//减少FireMoney数量
    {
        fireMoneyCount -= amount;
        PlayerPrefs.SetInt("FireMoney", fireMoneyCount);
        PlayerPrefs.Save();
        UpdateFireMoneyText();
    }
    // 获取当前FireMoney数量
    public int GetFireMoneyCount()
    {
        return fireMoneyCount;
    }
    
    public void LoadFireMoneyCount()
    {
        fireMoneyCount = PlayerPrefs.GetInt("FireMoney");
        UpdateFireMoneyText();
    }
}