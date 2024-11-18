using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

// 이동하는 Enemy의 상태 구현
namespace Unity.FPS.AI
{
    // 상태
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

        // 애니메이션 파라미터
        private const string k_Attack = "Attack";
        private const string k_MoveSpeed = "MoveSpeed";
        private const string k_Alerted = "Alerted";
        private const string k_OnDamaged= "OnDamaged";
        private const string k_Death= "Death";

        // 데미지 - 이펙트
        public ParticleSystem[] randomHitSparks;

        AudioSource audioSource;
        EnemyController enemyController;

        // Detected
        public ParticleSystem[] detectedVfx;
        public AudioClip detectedSound;


        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateState();
            UpdateAIStateTransition();
            // 속도에 따른 애니메이션 사운드 효과
            float moveSpeed = enemyController.Agent.velocity.magnitude;
            animator.SetFloat(k_MoveSpeed, moveSpeed);

            audioSource.pitch = pitchSpeed.GetValueFromRatio(moveSpeed / enemyController.Agent.speed);
        }

        void Init()
        {
            enemyController = GetComponent<EnemyController>();
            audioSource = GetComponent<AudioSource>();

            enemyController.Damaged += OnDamaged;
            enemyController.OnDetectedTarget += OnDetected;
            enemyController.OnLostTarget += OnLost;

            audioSource.clip = moveSound;
            audioSource.Play();

            aiState = AIState.Patrol;
        }

        void OnDamaged()
        {
            // 스파크 파티클 - 랜덤하게 하나 선택해서 플레이
            if (randomHitSparks.Length > 0)
            {
                int random = Random.Range(0, randomHitSparks.Length);

                randomHitSparks[random].Play();
            }

            animator.SetTrigger(k_OnDamaged);
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
                    enemyController.SetNavGetDestination(enemyController.knownDetectedTarget.transform.position);
                    enemyController.OrientToward(enemyController.knownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.knownDetectedTarget.transform.position);
                    break;
                case AIState.Attack:
                    enemyController.OrientToward(enemyController.knownDetectedTarget.transform.position);
                    enemyController.OrientWeaponsToward(enemyController.knownDetectedTarget.transform.position);
                    enemyController.TryAttack(enemyController.knownDetectedTarget.transform.position);
                    break;
            }
        }

        void OnDetected()
        {
            aiState = AIState.Follow;    

            // Vfx
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Play();
            }

            // Sfx
            if (detectedSound != null)
            {
                AudioUtility.instacne.CreateSfx(detectedSound, transform.position, 1f);
            }

            animator.SetBool(k_Alerted, true);
        }
        void OnLost()
        {
            aiState = AIState.Patrol;
            // Vfx
            for (int i = 0; i < detectedVfx.Length; i++)
            {
                detectedVfx[i].Stop();
            }

            animator.SetBool(k_Alerted, false);
        }

        // 상태 변경
        void UpdateAIStateTransition()
        {
            switch (aiState)
            {
                case AIState.Patrol:
                    
                    break;
                case AIState.Follow:
                    if (enemyController.IsTargetInAttackRange && enemyController.IsSeeingTarget)
                    {
                        aiState = AIState.Attack;

                        // 정지
                        enemyController.SetNavGetDestination(transform.position);
                    }
                    break;
                case AIState.Attack:
                    if (enemyController.IsTargetInAttackRange == false)
                    {
                        aiState = AIState.Follow;
                    }
                    break;
            }
        }
    }
}