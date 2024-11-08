using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TimeSelfDestruct 부착한 게임 오브젝트는 지정된 시간에 킬
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