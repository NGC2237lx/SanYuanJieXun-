using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skeletonCheckPlayer : MonoBehaviour
{
    [SerializeField] Skeleton skeleton;
    [SerializeField] private GameObject player;
    // 当其他2D碰撞体进入此触发器时调用 (用于检测玩家进入范围)
    private void Start()
    {
        skeleton = GetComponentInParent<Skeleton>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        print("Player detected");
        if (collision.gameObject.CompareTag("Player"))
        {
            player = collision.gameObject;
            skeleton.playerDetectedInTrigger = true;
            skeleton.player = player;
        }
    }

    // 当其他2D碰撞体退出此触发器时调用
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            skeleton.playerDetectedInTrigger = false;
            skeleton.player = null;
            player = null;
        }
    }
}
