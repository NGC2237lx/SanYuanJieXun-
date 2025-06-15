using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class skeletonCheckPlayer : MonoBehaviour
{
    [SerializeField] Skeleton skeleton;
    [SerializeField] private GameObject player;
    // ������2D��ײ�����˴�����ʱ���� (���ڼ����ҽ��뷶Χ)
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

    // ������2D��ײ���˳��˴�����ʱ����
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
