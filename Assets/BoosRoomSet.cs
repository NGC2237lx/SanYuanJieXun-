using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoosRoomSet : MonoBehaviour
{
    private GameObject player;
    [SerializeField] private Transform bossRoomEntrance; // Boss房间入口位置

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        player.transform.position = bossRoomEntrance.position; // 将玩家传送到Boss房间
    }
}
