using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 게임에 등장하는 Actor
namespace Unity.FPS.Game
{
    public class Actor : MonoBehaviour
    {
        // 소속 - 아군, 적군 구분
        public int affiliation;
        // 조준점 
        public Transform aimPoint;

        ActorManager actorManager;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Init()
        {
            // Actor 리스트에 등록
            actorManager = FindObjectOfType<ActorManager>();

            if (!actorManager.Actors.Contains(this))
            {
                actorManager.Actors.Add(this);
            }


        }

        private void OnDestroy()
        {
            // 리스트 제거
            if (actorManager != null)
            {
                actorManager.Actors.Remove(this);
            }

        }
    }
}