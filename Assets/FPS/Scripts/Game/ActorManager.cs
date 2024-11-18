using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���ӿ� �����ϴ� Actor���� �����ϴ� �Ŵ���
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

        // �÷��̾� ����
        public void SetPlayer(GameObject player) => Player = player;
    }
}