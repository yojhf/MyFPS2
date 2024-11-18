using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.Events;

// 적 디텍팅 구현
namespace Unity.FPS.AI
{
    public class DetectionModule : MonoBehaviour
    {
        // 적을 감지하면 등록된 함수 호출
        public UnityAction OnDetectedTarget;
        // 적을 놓치면 등록된 함수 호출
        public UnityAction OnLostTarget;

        public GameObject KnownDetectedTarget { get; private set; }
        public bool HadKnownTarget { get; private set; }
        public bool IsSeeingTarget {  get; private set; }
        
        public Transform detectionSourcePoint;
        public float detectionRange = 20f;

        public float knownTargetTimeout = 4f;
        private float timeLastSeenTarget = Mathf.NegativeInfinity;

        ActorManager actorManager;

        public float attackRange = 10f;
        public bool IsTargetInAttackRange { get; private set; } 

        // Start is called before the first frame update
        void Start()
        {
            actorManager = FindObjectOfType<ActorManager>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        // 적 감지
        public void OnDetected()
        {
            OnDetectedTarget?.Invoke();
        }
        // 적을 놓침
        public void OnLost()
        {
            OnLostTarget?.Invoke();
        }

        // 디텍팅
        public void HandleTargetDetection(Actor actor, Collider[] selfcollider)
        {

            if (KnownDetectedTarget != null && !IsSeeingTarget && (Time.time - timeLastSeenTarget) > knownTargetTimeout)
            {
                KnownDetectedTarget = null;
            }

            IsSeeingTarget = false;

            float sqrDectectionRange = detectionRange * detectionRange;
            float closeSqrdistance = Mathf.Infinity;

            foreach (var otherActor in actorManager.Actors)
            {
                if (otherActor.affiliation == actor.affiliation)
                {
                    continue;
                }

                float sqrDistance = (otherActor.aimPoint.position - detectionSourcePoint.position).sqrMagnitude;


                if (sqrDistance < sqrDectectionRange && sqrDistance < closeSqrdistance)
                {
                    RaycastHit[] hits = Physics.RaycastAll(detectionSourcePoint.position, (otherActor.aimPoint.position - detectionSourcePoint.position).normalized, 
                        detectionRange, -1, QueryTriggerInteraction.Ignore);

                    RaycastHit closestHit = new RaycastHit();

                    closestHit.distance = Mathf.Infinity;
                    bool foundValidHit = false;

                    foreach (var hit in hits)
                    {
                        if (hit.distance < closestHit.distance && !selfcollider.Contains(hit.collider))
                        {
                            closestHit = hit;
                            foundValidHit = true;

                        }
                    }

                    // 적 감지
                    if (foundValidHit)
                    {
                        Actor hitActor = closestHit.collider.GetComponentInParent<Actor>();

                        if (hitActor == otherActor)
                        {
                            IsSeeingTarget = true;
                            closeSqrdistance = sqrDistance;

                            timeLastSeenTarget = Time.time;

                            KnownDetectedTarget = otherActor.aimPoint.gameObject;
                        }
                    }

                }
            }

            if (KnownDetectedTarget != null &&
                Vector3.Distance(transform.position, KnownDetectedTarget.transform.position) <= attackRange)
            {
                IsTargetInAttackRange = true;
            }
            else
            {
                IsTargetInAttackRange = false;
            }


            // 적을 발견한 순간
            if (HadKnownTarget == false && KnownDetectedTarget != null)
            {
                OnDetected();
            }

            // 적을 놓치는 순간
            if (HadKnownTarget == true && KnownDetectedTarget == null)
            {
                OnLost();
            }

            HadKnownTarget = KnownDetectedTarget != null;


        }

    }
}