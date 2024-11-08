using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TimeSelfDestruct ������ ���� ������Ʈ�� ������ �ð��� ų
namespace Unity.FPS.Game
{
    public class TimeSelfDestruct : MonoBehaviour
    {
        public float killTime = 1f;
        private float spawnTime;

        // Start is called before the first frame update
        void Start()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            if((spawnTime + killTime) <= Time.time)
            {
                Destroy(gameObject);
            }
        }

    }
}