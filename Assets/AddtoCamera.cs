using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Com.LuisPedroFonseca.ProCamera2D
{
    public class AddtoCamera : MonoBehaviour
    {
        ProCamera2D pc;
        
        private GameObject player;

        void Awake()
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                AddplayertoCamera();
            }
        }

        void AddplayertoCamera()
        {
            pc = FindObjectOfType<ProCamera2D>();
            if (pc != null && pc.CameraTargets.Count == 0)
            {
                Transform newPos = player.transform;

                pc.AddCameraTarget(newPos);
            }
        }
    }
}

