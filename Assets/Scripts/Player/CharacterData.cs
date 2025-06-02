using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterData : MonoBehaviour
{
    [SerializeField] private int health;
    [SerializeField] private bool isDead;
    [SerializeField] private int maxHealth = 5; // 添加最大生命值

    private GameManager gameManager;
    private CharacterEffect effecter;
    private Animator animator;

    private bool isLeak;

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
        if (!isDead && health < maxHealth)
        {
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

    public bool GetDeadStatement()
    {
        CheckIsDead();
        return isDead;
    }

    public void Die()
    {
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Hero Detector"), LayerMask.NameToLayer("Enemy Detector"), true);
        isDead = true;
        animator.SetTrigger("Dead");
    }

    public void Respawn()
    {
        FindObjectOfType<HazardRespawn>().Respawn();
    }

    public void SetRespawnData(int health)
    {
        if (health > 0)
        {
            this.health = health;
            animator.ResetTrigger("Dead");
            isDead = false;
        }
    }
}