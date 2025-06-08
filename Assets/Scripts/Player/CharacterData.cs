using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private bool isDead;
    [SerializeField] private int maxHealth = 5; // 添加最大生命值
    [SerializeField] private float respawnDelay = 3f; // 重生等待时间

    [SerializeField] private FireMoneyCollector fc;
    private GameManager gameManager;
    private CharacterEffect effecter;
    private Animator animator;

    private bool isLeak;

    private bool isTemple;

    private Vector3 temple_birth_place;

    private Vector3 respawnPosition; // 重生位置
    private int respawnHealth; // 重生时的生命值



    private void Start()
    {
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
        effecter = FindObjectOfType<CharacterEffect>();
    }

    private void Update()
    {
        CheckIsDead();
        CheckLeakHealth();
    }

    // 新增：城隍庙治疗触发方法
    public void HealAtTemple()
    {
        temple_birth_place = transform.position;
        if (!isDead && health < maxHealth && !isTemple)
        {
            isTemple = true;
            health = maxHealth; // 完全恢复生命值
            Debug.Log($"在城隍庙治疗，生命值恢复至: {health}");
        }
    }

    // 以下保持原有函数不变
    private void CheckLeakHealth()
    {
        if (health == 1 && !isLeak)
        {
            isLeak = true;
            effecter.DoEffect(CharacterEffect.EffectType.LowHealthLeak, true);
        }
        else if (health != 1 && isLeak)
        {
            isLeak = false;
            effecter.DoEffect(CharacterEffect.EffectType.LowHealthLeak, false);
        }
    }

    private void CheckIsDead()
    {
        if (health <= 0 && !isDead)
        {
            Die();
        }
    }

    public void LoseHealth(int health)
    {
        this.health -= health;
    }

    public int GetCurrentHealth()
    {
        return health;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool GetDeadStatement()
    {
        CheckIsDead();
        return isDead;
    }

    public void Die()
    {
        if (isDead) return;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Hero Detector"),
                                      LayerMask.NameToLayer("Enemy Detector"), true);
        isDead = true;


        // 触发死亡动画
        animator.SetTrigger("Dead");

        // 开始重生协程
        StartCoroutine(RespawnAfterDelay());
    }

    // 重生协程
    private IEnumerator RespawnAfterDelay()
    {
        // 等待死亡动画播放完成
        yield return new WaitForSeconds(respawnDelay);

        // 播放重生动画
        animator.SetTrigger("Respawn");

        // 等待重生动画播放
        //yield return new WaitForSeconds(1f); // 根据实际动画长度调整

        // 实际重生逻辑
        CompleteRespawn();
    }

    // 完成重生
    private void CompleteRespawn()
    {
        if (!isTemple)
        {
            DungeonGenerator dg = FindObjectOfType<DungeonGenerator>();
            //重置地牢
            dg.ResetDungeon();
            // 重置位置
            Transform birthplace = dg.Getbirthplace();
            //调整这个脚本挂载到的对象的位置
            transform.position = new Vector3(birthplace.position.x, birthplace.position.y, transform.position.z);
        }
        else
        {
            transform.position = temple_birth_place;
        }

        // 恢复生命值
        health = maxHealth;


        // 扣钱 
        fc.ResetMoney();
        // 重置状态
        isDead = false;
        animator.ResetTrigger("Dead");

        isTemple = false;

        // 恢复物理碰撞
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Hero Detector"),
                                     LayerMask.NameToLayer("Enemy Detector"), false);

        Debug.Log("角色已重生");
    }

    // 设置重生点
    public void SetRespawnPoint(Vector3 position, int healthAmount)
    {
        respawnPosition = position;
        respawnHealth = healthAmount;
        Debug.Log($"重生点已更新至: {position}, 重生生命值: {healthAmount}");
    }

    // 设置重生数据
    public void SetRespawnData(int health)
    {
        // 如果生命值大于0
        if (health > 0)
        {
            // 将生命值设置为传入的值
            this.health = health;
            // 重置死亡动画触发器
            animator.ResetTrigger("Dead");
            isDead = false;
        }
    }
    public void SetMaxHealth(int maxHealth)
    {
        this.maxHealth = maxHealth;
        health = maxHealth;
    }
    public void SaveCharacterData()
    {
        PlayerPrefs.SetInt("Health", health);
        PlayerPrefs.SetInt("MaxHealth", maxHealth);
        PlayerPrefs.Save();
        Debug.Log("角色数据已保存");
    }
    public void LoadCharacterData()
    {
        health=PlayerPrefs.GetInt("Health");
        maxHealth=PlayerPrefs.GetInt("MaxHealth");
        Debug.Log("角色数据已加载");
    }
}