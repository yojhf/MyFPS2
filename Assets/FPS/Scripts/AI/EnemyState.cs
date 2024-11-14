using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

// �̵��ϴ� Enemy�� ���� ����
namespace Unity.FPS.AI
{
    // ����
    public enum AIState
    {
        Patrol,
        Follow,
        Attack
    }

    public class EnemyState : MonoBehaviour
    {
        public AIState aiState { get; private set; }   
        public Animator animator;
        
        public AudioClip moveSound;   
        public MinMaxFloat pitchSpeed;

        // �ִϸ��̼� �Ķ����
        private const string k_Attack = "Attack";
        private const string k_MoveSpeed = "MoveSpeed";
        private const string k_Alerted = "Alerted";
        private const string k_OnDamaged= "OnDamaged";
        private const string k_Death= "Death";

        AudioSource audioSource;
        EnemyController enemyController;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateState();
        }

        void Init()
        {
            enemyController = GetComponent<EnemyController>();
            audioSource = GetComponent<AudioSource>();

            enemyController.Damaged += OnDamaged;

            audioSource.clip = moveSound;
            audioSource.Play();

            aiState = AIState.Patrol;
        }

        void OnDamaged()
        {

        }

        void UpdateState()
        {
            switch (aiState)
            { 
                case AIState.Patrol:
                    enemyController.UpdatePathDestination(true);
                    enemyController.SetNavGetDestination(enemyController.GetDestination());
                    break;
                case AIState.Follow:

                    break;
                case AIState.Attack:

                    break;
            }
        }
    }
}