using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

// �߻�ü ǥ����
namespace Unity.FPS.Gameplay
{
    public class ProjectileStandard : ProjectileBase
    {
        private ProjectileBase projectileBase;
        private float maxLiftTime = 5f;

        // �̵�
        public Transform root;
        public Transform tip;
        public float speed = 20f;
        public float gravityDown = 0f;

        private Vector3 velocity;
        private Vector3 lastRootPos;
        private float shootTime;

        // �浹
        // �浹 �˻��ϴ� ��ü�� �ݰ�
        public float radius = 0.01f;
        public LayerMask hitTableLayers = -1;
        private List<Collider> ignoreCols = new List<Collider>();

        //�浹����
        public GameObject impactVfxPrefab;
        public float impactVfxLifeTime = 5f;
        private float impactVfxSpawnOffset = 0.1f;

        // Ÿ����
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

            // �浹
            RaycastHit hit = new RaycastHit();
            // hit�� �浹ü�� ã�Ҵ��� ����
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

            // hit�� �浹ü ã��
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

        // shoot �� ����
        new void OnShoot()
        {
            velocity = transform.forward * speed;

            transform.position += projectileBase.InheritedMuzzleVelocity; 

            lastRootPos = root.position;

            // �浹 ���� ����Ʈ ���� - projectile�� �߻��ϴ� �ڽ��� �浹ü�� �����ͼ� ���
            Collider[] ownerCol = projectileBase.Owner.GetComponentsInChildren<Collider>();

            ignoreCols.AddRange(ownerCol);

            // ������Ÿ���� ���� ��� ���ư��� ����
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

        // ������ hit���� Ȯ��
        bool IsHitValid(RaycastHit hit)
        {
            // IgnoreHitDectection ������Ʈ�� ���� �ݶ��̴� ����
            if(hit.collider.GetComponent<IgnoreHitDectection>() != null)
            {
                return false;
            }
            // IgnoreCollider�� ���� �� �ݶ��̴� ����
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

        // Hit ���� : ������, Vfx, Sfx
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
                // �浹��ġ�� ���ӿ�����Ʈ�� �����ϰ� AudioSource ������Ʈ�� �߰��ؼ� ������ Ŭ���� �÷���

                AudioUtility.instacne.CreateSfx(impactSfxSound, point, 1f, 3f);
            }

            Destroy(gameObject);
        }
    }
}