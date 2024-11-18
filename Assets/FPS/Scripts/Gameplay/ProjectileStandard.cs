using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

// 발사체 표준형
namespace Unity.FPS.Gameplay
{
    public class ProjectileStandard : ProjectileBase
    {
        private ProjectileBase projectileBase;
        private float maxLiftTime = 5f;

        // 이동
        public Transform root;
        public Transform tip;
        public float speed = 20f;
        public float gravityDown = 0f;

        private Vector3 velocity;
        private Vector3 lastRootPos;
        private float shootTime;

        // 충돌
        // 충돌 검사하는 구체의 반경
        public float radius = 0.01f;
        public LayerMask hitTableLayers = -1;
        private List<Collider> ignoreCols = new List<Collider>();

        //충돌연출
        public GameObject impactVfxPrefab;
        public float impactVfxLifeTime = 5f;
        private float impactVfxSpawnOffset = 0.1f;

        // 타격음
        public AudioClip impactSfxSound;

        // 
        public float damage = 20f;
        DamageArea damageArea;


        private void Update()
        {
            transform.position += velocity * Time.deltaTime;

            if(gravityDown > 0f)
            {
                velocity += Vector3.down * gravityDown * Time.deltaTime;
            }

            // 충돌
            RaycastHit hit = new RaycastHit();
            // hit한 충돌체를 찾았는지 여부
            bool foundHit = false;

            hit.distance = Mathf.Infinity;

            // Sphere Cast
            Vector3 displacement = tip.position - lastRootPos;

            RaycastHit[] hits = Physics.SphereCastAll(lastRootPos, radius, displacement.normalized, 
                                                        displacement.magnitude, hitTableLayers, 
                                                        QueryTriggerInteraction.Collide);

            foreach(var colhit in hits)
            {
                if (IsHitValid(colhit) && colhit.distance < hit.distance)
                {
                    foundHit = true;
                    hit = colhit;
                }
            }

            // hit한 충돌체 찾음
            if(foundHit == true)
            {
                if(hit.distance <= 0f)
                {
                    hit.point = root.position;
                    hit.normal = -transform.forward;
                }    

                OnHit(hit.point, hit.normal, hit.collider);
            }

            lastRootPos = root.position;

            
        }

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
            damageArea = GetComponent<DamageArea>();

            Destroy(gameObject, maxLiftTime);
        }

        // shoot 값 설정
        new void OnShoot()
        {
            velocity = transform.forward * speed;

            transform.position += projectileBase.InheritedMuzzleVelocity; 

            lastRootPos = root.position;

            // 충돌 무시 리스트 생성 - projectile을 발사하는 자신의 충돌체를 가져와서 등록
            Collider[] ownerCol = projectileBase.Owner.GetComponentsInChildren<Collider>();

            ignoreCols.AddRange(ownerCol);

            // 프로젝타일이 벽을 뚦고 날아가는 버그
            PlayerWeaponsManager weaponsManager = projectileBase.Owner.GetComponent<PlayerWeaponsManager>();

            if(weaponsManager != null)
            {
                Vector3 cameraToMuzzle = projectileBase.InitPos - weaponsManager.weaponCam.transform.position;

                if(Physics.Raycast(weaponsManager.weaponCam.transform.position, cameraToMuzzle.normalized, 
                                    out RaycastHit hit, cameraToMuzzle.magnitude, hitTableLayers, 
                                    QueryTriggerInteraction.Collide))
                {
                    if(IsHitValid(hit))
                    {
                        OnHit(hit.point, hit.normal, hit.collider);
                    }
                }
            }

        }

        // 유요한 hit인지 확인
        bool IsHitValid(RaycastHit hit)
        {
            // IgnoreHitDectection 컴포넌트를 가진 콜라이더 무시
            if(hit.collider.GetComponent<IgnoreHitDectection>() != null)
            {
                return false;
            }
            // IgnoreCollider에 포함 된 콜라이더 무시
            if (ignoreCols != null && ignoreCols.Contains(hit.collider))
            {
                return false;
            }
            // trigger Collider
            if(hit.collider.isTrigger && hit.collider.GetComponent<Damageable>() == null)
            {
                return false;
            }

            return true;
        }

        // Hit 구현 : 데미지, Vfx, Sfx
        void OnHit(Vector3 point, Vector3 nomal, Collider collider)
        {
            if (damageArea != null)
            {
                damageArea.InflictDamageArea(damage, point, hitTableLayers, QueryTriggerInteraction.Collide, projectileBase.Owner);
            }
            else
            {
                Damageable damageable = collider.GetComponent<Damageable>();

                if (damageable != null)
                {
                    damageable.InflictDamage(damage, false, projectileBase.Owner);
                }
            }




            // Vfx
            if (impactVfxPrefab != null)
            {
                GameObject impactObj = Instantiate(impactVfxPrefab, point + (nomal * impactVfxSpawnOffset), Quaternion.LookRotation(nomal));
                
                if(impactVfxLifeTime > 0f)
                {
                    Destroy(impactObj, impactVfxLifeTime);
                }
                
            }

            // Sfx
            if(impactSfxSound)
            {
                // 충돌위치에 게임오브젝트를 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이

                AudioUtility.instacne.CreateSfx(impactSfxSound, point, 1f, 3f);
            }

            Destroy(gameObject);
        }
    }
}