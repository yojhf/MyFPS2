using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.ShaderGraph.Internal;

// ������ �����ϴ� Ŭ����
namespace Unity.FPS.Game
{
    public enum WeaponAimType
    {
        Nomal,
        Snipe
    }

    // ���� �߻�
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge
    }

    // ũ�ν���� �����ϴ� Ŭ����
    [Serializable]
    public class CrossHairData
    {
        public Sprite crossHairSprit;
        public Color crossHairColor;
        public float crossHairSize;
    }

    public class WeaponCon : MonoBehaviour
    {
        // AimingType
        public WeaponAimType aimType;
        // Shooting
        public WeaponShootType shootType;

        // ���� Ȱ��ȭ, ��Ȱ��ȭ
        private GameObject weaponRoot;

        // ���� ����
        public GameObject Owner { get; set; }
        // ���⸦ ������ �������� ������
        public GameObject SourcePrefab { get; set; }
        // ���� Ȱ��ȭ ����
        public bool IsWeaponActive { get; private set; }

        // �⺻
        public CrossHairData crossHairDefault;
        // �� ����
        public CrossHairData crossHairTarget;

        public AudioClip switchWeaponSfx;
        AudioSource shootAudioSource;

        // ���� �� ���� ������
        public float aimZoomRatio = 1f;
        // ���� �� ���� ��ġ ��
        public Vector3 aimOffset;

        // ������ �� �ִ� �ִ� �Ѿ˰���
        public float maxBullet = 8f;
        private float currentBullet;
        public float CurrentAmmoRatio => currentBullet / maxBullet;

        // �߻� ����
        public float bulletChargeDelay = 0.5f;
        // ������ �߻� �ð�
        private float lastTimeShoot;

        // Vfx, Sfx
        // �ѱ� ��ġ
        public Transform weaponMuzzle;
        // �ѱ� �߻� ȿ��
        public GameObject muzzleFlashPrefab;
        public AudioClip shootSfx;

        // �ݵ�
        public float recoilForce = 0.5f;

        // Projectile
        public ProjectileBase projectilePrefab;
        
        public Vector3 MuzzleWorldVelocity { get; private set; }
        private Vector3 lastMuzzlePos;

        // �ѹ� �߻� �� źȯ ����
        public int bulletsPershot = 1;
        // �Ѿ��� ���������� ����
        public float bulletSpreadAnlge = 0f;

        // Charge : �߻� ��ư�� ������ ������ �߻�ü�� �����, �ӵ��� ������ ���� Ŀ����
        public float CurrentCharge { get; private set; }
        public bool IsCharge {  get; private set; }
        // ���� ���� ��ư�� ������ ���� �ʿ��� ammo��
        public float useCharge = 1f;
        // �����ϰ� �ִµ��� �Һ�Ǵ� ammo ��
        public float rateCharge = 1f;
        // ���� �ð� max
        private float maxChargeDuration = 2f;
        // ���ܤ�;��;��
        public float lastChargeTriggerTime;

        // Reload
        // �ʴ� �������Ǵ� ��
        public float reloadRate = 1f;
        public float reloadDelay = 2f;
        public bool autoReload = true;

        private void Awake()
        {
            Init();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            UpdateCharge();
            UpdateBullet();
            UpdateReload();
            Reload();
        }

        void Init()
        {
            weaponRoot = transform.GetChild(0).gameObject;
            shootAudioSource = GetComponent<AudioSource>();
            currentBullet = maxBullet;
            lastTimeShoot = Time.time;
        }

        void UpdateBullet()
        {
            if(Time.deltaTime > 0f)
            {
                MuzzleWorldVelocity = (weaponMuzzle.position - lastMuzzlePos) / Time.deltaTime;

                lastMuzzlePos = weaponMuzzle.position;
            }
        }

        public void ShowWeapon(bool show)
        {
            weaponRoot.SetActive(show);

            // this ����� ����
            if(show == true && switchWeaponSfx != null)
            {
                // ���� ���� ȿ����
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }

            IsWeaponActive = show;
        }

        // Ű �Է¿� ���� Shoot ����
        public bool HandleShootInputs(bool intputDown, bool inputHeld, bool inputUp)
        {
            switch(shootType)
            {
                case WeaponShootType.Manual:
                    if(intputDown == true)
                    {
                        return TryShoot();
                    }
                    break;
                case WeaponShootType.Automatic:
                    if (inputHeld == true)
                    {
                        return TryShoot();                       
                    }
                    break;
                case WeaponShootType.Charge:
                    if (inputHeld == true)
                    {
                        // ���� ����
                        TryBeginCharge();        
                    }
                    if(inputUp == true)
                    {
                        return TryReleaseCharge();
                    }
                    break;

            }

            return false;
        }

        void UpdateCharge()
        {
            if(IsCharge)
            {
                if(CurrentCharge < 1f)
                {
                    // ���� �����ִ� ������
                    float chargeLeft = 1f - CurrentCharge;
                    // �̹� ������ ������
                    float chargeAdd = 0f;

                    if(maxChargeDuration <= 0f)
                    {
                        // �ѹ��� Ǯ����
                        chargeAdd = chargeLeft;
                    }
                    else
                    {
                        chargeAdd = (1f / maxChargeDuration) * Time.deltaTime;
                    }

                    // �����ִ� ���������� �۾ƾ� ��
                    chargeAdd = Mathf.Clamp(chargeAdd, 0f, chargeLeft);

                    // chargeAdd ��ŭ Ammo�� �Һ�
                    float ammoChargeReq = chargeAdd * rateCharge;

                    if(ammoChargeReq <= currentBullet)
                    {
                        UseAmmo(ammoChargeReq);

                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdd);
                    }
                }
            }

        }
        // �߻� ����
        void TryBeginCharge()
        {
            if (IsCharge == false && currentBullet >= useCharge
                && (lastTimeShoot + bulletChargeDelay) < Time.time)
            {
                UseAmmo(useCharge);

                lastChargeTriggerTime = Time.time;
                IsCharge = true;
            }


        }
        // �߻� ��
        bool TryReleaseCharge()
        {
            if (IsCharge)
            {
                HandleShoot();

                CurrentCharge = 0f;
                IsCharge = false;
                return true;
            }

            return false;
        }

        void UseAmmo(float amount)
        {
            currentBullet = Mathf.Clamp(currentBullet - amount, 0f, maxBullet);

            lastTimeShoot = Time.time;
        }

        bool TryShoot()
        {
            if(currentBullet >= 1f && (lastTimeShoot + bulletChargeDelay) < Time.time)
            {
                currentBullet -= 1f;
                HandleShoot();

                return true;
            }

            return false;
        }

        // �� �߻� ����
        void HandleShoot()
        {
            for(int i = 0; i < bulletsPershot; i++)
            {
                Vector3 shootdir = GetShootDirection(weaponMuzzle);

                // Projectile ����
                ProjectileBase projectile = Instantiate(projectilePrefab, weaponMuzzle.position, Quaternion.LookRotation(shootdir));

                projectile.Shoot(this);
            }


            // Vfx
            if(muzzleFlashPrefab != null)
            {
                GameObject effectGo = Instantiate(muzzleFlashPrefab, weaponMuzzle.position, weaponMuzzle.rotation, weaponMuzzle);

                Destroy(effectGo, 2f);
            }

            // Sfx
            if(shootSfx != null)
            {
                shootAudioSource.PlayOneShot(shootSfx);
            }

            lastTimeShoot = Time.time;

        }

        // projectile ���󰡴� ����
        Vector3 GetShootDirection(Transform shootTransform)
        {
            float spreadAngleRatio = bulletSpreadAnlge / 180f;

            return Vector3.Lerp(shootTransform.forward, UnityEngine.Random.insideUnitSphere, spreadAngleRatio);
        }

        // Reload - auto
        void UpdateReload()
        {
            if(autoReload && currentBullet < maxBullet || !IsCharge)
            {
                if(lastTimeShoot + reloadDelay < Time.time)
                {
                    currentBullet += reloadRate * Time.deltaTime;
                    currentBullet = Mathf.Clamp(currentBullet, 0, maxBullet);

                }
            }
        }

        // Reload - solid
        public void Reload()
        {
            if (autoReload || currentBullet >= maxBullet || IsCharge)
            {
                return;       
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                currentBullet = maxBullet;
            }

        }

    }
}