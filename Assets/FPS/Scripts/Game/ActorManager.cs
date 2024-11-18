using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 게임에 등장하는 Actor들을 관리하는 매니저
namespace Unity.FPS.Game
{
    public class ActorManager : MonoBehaviour
    {
        public List<Actor> Actors { get; private set; }
        public GameObject Player { get; private set; }


        private void Awake()
        {
            Actors = new List<Actor>();
        }
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        // 플레이어 셋팅
        public void SetPlayer(GameObject player) => Player = player;
    }
}