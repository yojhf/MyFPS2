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

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateDamage();
            wasDamaged = false;
        }

        void Init()
        {
            health = GetComponent<Health>();
            Agent = GetComponent<NavMeshAgent>();
            bodyFlash = new MaterialPropertyBlock();

            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                {
                    if (renderer.sharedMaterials[i] == bodyMaterial)
                    {
                        bodyRenderer.Add(new RendererIndexData(renderer, i));
                    }

                }


            }
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
    }
}