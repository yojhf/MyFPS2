using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using System;
using UnityEngine.Events;
using UnityEngine.AI;

namespace Unity.FPS.AI
{
    [Serializable]
    public class RendererIndexData
    {
        public Renderer renderer;
        public int matIndex;

        public RendererIndexData(Renderer _renderer, int _matIndex)
        {
            renderer = _renderer;
            matIndex = _matIndex;
        }   
    }

    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private GameObject explorsionEffect;
        [SerializeField] private Transform effectPos;

        // damage
        public UnityAction Damaged;

        // sfx
        public AudioClip damageSfx;

        // vfx
        [GradientUsage(true)]
        public Gradient hitGradient;
        public Material bodyMaterial;

        private List<RendererIndexData> bodyRenderer = new List<RendererIndexData>();

        public float flashDuration = 0.5f;
        private float lastTimeDamaged = float.NegativeInfinity;
        private bool wasDamaged = false;

        // Patorl
        public NavMeshAgent Agent { get; private set; }
        public PatrolPath PatrolPath { get; set; }
        private int pathDestinationIndex;
        // 도착판정
        private float pathRadius = 1f;

        MaterialPropertyBlock bodyFlash;
        Health health;

        // Detection
        Actor actor;
        private Collider[] selfCols;

        public DetectionModule DetectionModule { get; private set; }

        public GameObject knownDetectedTarget => DetectionModule.KnownDetectedTarget;
        public bool IsSeeingTarget => DetectionModule.IsSeeingTarget;
        public bool HadKnownTarget => DetectionModule.HadKnownTarget;

        public Material eyeColorMat;
        [ColorUsage(true, true)] public Color defaultEyeColor;
        [ColorUsage(true, true)] public Color attackEyeColor;

        // eye Material을 가지고 있는 렌더러 데이터
        private RendererIndexData eyeRendererData;
        private MaterialPropertyBlock eyeColorMatPro;

        public UnityAction OnDetectedTarget;
        public UnityAction OnLostTarget;

        // Attack
        private float orientSpeed = 10f;
        public bool IsTargetInAttackRange => DetectionModule.IsTargetInAttackRange;
        public bool swapNextWeapon = false;
        public float delayAfterWeaponSwap = 0f;
        private float lastTimeWeaponSwap = Mathf.NegativeInfinity;

        public int currentWeaponIndex;
        private WeaponCon currentWeapon;
        private WeaponCon[] weapons;

        public UnityAction OnAttack;

        // enemyManager
        EnemyManager enemyManager;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            DetectionModule.HandleTargetDetection(actor, selfCols);
            UpdateDamage();
            wasDamaged = false;
        }

        void Init()
        {
            var detectionModules = GetComponentsInChildren<DetectionModule>();

            DetectionModule = detectionModules[0];
            DetectionModule.OnDetectedTarget += OnDetected;
            DetectionModule.OnLostTarget += OnLost;

            health = GetComponent<Health>();
            Agent = GetComponent<NavMeshAgent>();
            actor = GetComponent<Actor>();
            selfCols = GetComponentsInChildren<Collider>();
            enemyManager = FindObjectOfType<EnemyManager>();
            enemyManager.RegisterEnemy(this);

            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    // body
                    if (renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }

                    // eye
                    if (renderer.sharedMaterials[i] == eyeColorMat)
                    {
                        eyeRendererData = new RendererIndexData(renderer, i);
                    }
                }
            }

            bodyFlash = new MaterialPropertyBlock();

            if (eyeRendererData.renderer != null)
            {
                eyeColorMatPro = new MaterialPropertyBlock();

                eyeColorMatPro.SetColor("_EmissionColor", defaultEyeColor);

                eyeRendererData.renderer.SetPropertyBlock(eyeColorMatPro, eyeRendererData.matIndex);
            }


            // 무기 초기화
            FindAndInitAllWeapon();

            var weapon = GetCurrentWeapon();
            weapon.ShowWeapon(true);
        }

        void OnDamaged(float damage, GameObject damageSource)
        {
            if(damageSource != null && damageSource.GetComponent<EnemyController>() == null)
            {
                Damaged?.Invoke();

                lastTimeDamaged = Time.time;

                // Sfx
                if(damageSfx && wasDamaged == false)
                {
                    AudioUtility.instacne.CreateSfx(damageSfx, transform.position, 0f);
                }

                wasDamaged = true;

            }

        }
        void OnDie()
        {
            enemyManager.RemoveEnemy(this);

            GameObject deathEffect = Instantiate(explorsionEffect, effectPos.position, Quaternion.identity);

            Destroy(deathEffect, 5f);
        }

        void UpdateDamage()
        {
            Color curColor = hitGradient.Evaluate((Time.time - lastTimeDamaged) / flashDuration);

            bodyFlash.SetColor("_EmissionColor", curColor);

            foreach(var data in bodyRenderer)
            {
                data.renderer.SetPropertyBlock(bodyFlash, data.matIndex);
            }
        }

        // 패트롤 유효 확인
        bool IsPathVaild()
        {
            return PatrolPath && PatrolPath.wayPoints.Count > 0;
        }

        // 가장 가까운 wayPoint
        void SetPathDestination()
        {
            if(!IsPathVaild())
            {
                pathDestinationIndex = 0;

                return;
            }

            int closeIndex = 0;

            for (int i = 0; i < PatrolPath.wayPoints.Count; i++)
            {
                float distacne = PatrolPath.GetDistacne(transform.position, i);
                float closeDistacne = PatrolPath.GetDistacne(transform.position, closeIndex);

                if(distacne < closeDistacne)
                {
                    closeIndex = i;
                }
            }

            pathDestinationIndex = closeIndex;

            
        }

        public Vector3 GetDestination()
        {
            if (!IsPathVaild())
            {
                return transform.position;
            }

            return PatrolPath.GetPosition(pathDestinationIndex);
        }

        // 목표지점 설정 - Nav System
        public void SetNavGetDestination(Vector3 destination)
        {
            if(Agent != null)
            {
                Agent.SetDestination(destination);
            }
        }

        // 도착판정
        public void UpdatePathDestination(bool inversOrder = false)
        {
            float distacne = (transform.position - GetDestination()).magnitude;

            if(distacne <= pathRadius)
            {
                if (inversOrder)
                {
                    pathDestinationIndex--;
                }
                else
                {
                    pathDestinationIndex++;
                }

                if (pathDestinationIndex < 0)
                {
                    pathDestinationIndex += PatrolPath.wayPoints.Count;
                }
                if (pathDestinationIndex >= PatrolPath.wayPoints.Count)
                {
                    pathDestinationIndex -= PatrolPath.wayPoints.Count;
                }
             
            }
        }

        void OnDetected()
        {
            OnDetectedTarget?.Invoke();

            if (eyeRendererData.renderer != null)
            {
                eyeColorMatPro.SetColor("_EmissionColor", attackEyeColor);

                eyeRendererData.renderer.SetPropertyBlock(eyeColorMatPro, eyeRendererData.matIndex);
            }
        }

        void OnLost()
        {
            OnLostTarget?.Invoke();

            if (eyeRendererData.renderer != null)
            {

                eyeColorMatPro.SetColor("_EmissionColor", defaultEyeColor);

                eyeRendererData.renderer.SetPropertyBlock(eyeColorMatPro, eyeRendererData.matIndex);
            }
        }

        public void OrientToward(Vector3 lookPos, Transform enemy)
        {
            Vector3 lookDirect = Vector3.ProjectOnPlane(lookPos - enemy.position, Vector3.up).normalized;

            if (lookDirect.sqrMagnitude != 0)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDirect);

                enemy.rotation = Quaternion.Slerp(enemy.rotation, targetRot, orientSpeed * Time.deltaTime);
            }
        }

        // 가지고 있는 무기 찾고 초기화
        void FindAndInitAllWeapon()
        {
            if (weapons == null)
            {
                weapons = GetComponentsInChildren<WeaponCon>();

                for (int i = 0; i < weapons.Length; i++)
                {
                    weapons[i].Owner = gameObject;
                }
            }
        }

        // 지정한 인덱스에 해달하는 무기를 current로 지정
        void SetCurrentWeapon(int index)
        { 
            currentWeaponIndex = index;
            currentWeapon = weapons[currentWeaponIndex];

            if (swapNextWeapon)
            {
                lastTimeWeaponSwap = Time.time;
            }
            else
            {
                lastTimeWeaponSwap = Mathf.NegativeInfinity;
            }
        }

        // 현재 Active weapon 찾기
        public WeaponCon GetCurrentWeapon()
        {
            FindAndInitAllWeapon();

            if (currentWeapon == null)
            {
                SetCurrentWeapon(0);
            }

            return currentWeapon;
        }

        // 적에게 총구를 돌린다
        public void OrientWeaponsToward(Vector3 lookPos)
        {
            for (int i = 0; i < weapons.Length; i++)
            {
                Vector3 weaponForward = (lookPos - weapons[i].transform.position).normalized;

                weapons[i].transform.forward = weaponForward;


            }
        }

        // 공격 - 공격성공 / 실패
        public bool TryAttack(Vector3 targetPos)
        {
            if((lastTimeWeaponSwap + delayAfterWeaponSwap) >= Time.time)
            {
                return false;
            }

            // 무기 shoot
            bool didFire = GetCurrentWeapon().HandleShootInputs(false, true, false);

            if (didFire && OnAttack != null)
            {
                OnAttack?.Invoke();

                // 발사 시 다음 무기로 교체
                if (swapNextWeapon && weapons.Length > 1)
                {
                    int nextWeaponIndex = (currentWeaponIndex + 1) % weapons.Length;

                    SetCurrentWeapon(nextWeaponIndex);
                }
            }


            return true;
        }

    }
}