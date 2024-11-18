using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ���ӿ� �����ϴ� Actor
namespace Unity.FPS.Game
{
    public class Actor : MonoBehaviour
    {
        // �Ҽ� - �Ʊ�, ���� ����
        public int affiliation;
        // ������ 
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
            // Actor ����Ʈ�� ���
            actorManager = FindObjectOfType<ActorManager>();

            if (!actorManager.Actors.Contains(this))
            {
                actorManager.Actors.Add(this);
            }


        }

        private void OnDestroy()
        {
            // ����Ʈ ����
            if (actorManager != null)
            {
                actorManager.Actors.Remove(this);
            }

        }
    }
}