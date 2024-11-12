using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.ShaderGraph.Internal;

// 무기을 관리하는 클래스
namespace Unity.FPS.Game
{
    public enum WeaponAimType
    {
        Nomal,
        Snipe
    }

    // 무기 발사
    public enum WeaponShootType
    {
        Manual,
        Automatic,
        Charge
    }

    // 크로스헤어 관리하는 클래스
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

        // 무기 활성화, 비활성화
        private GameObject weaponRoot;

        // 무기 주인
        public GameObject Owner { get; set; }
        // 무기를 생성한 오리지널 프리펩
        public GameObject SourcePrefab { get; set; }
        // 무기 활성화 여부
        public bool IsWeaponActive { get; private set; }

        // 기본
        public CrossHairData crossHairDefault;
        // 적 포착
        public CrossHairData crossHairTarget;

        public AudioClip switchWeaponSfx;
        AudioSource shootAudioSource;

        // 조준 시 줌인 설정값
        public float aimZoomRatio = 1f;
        // 조준 시 무기 위치 값
        public Vector3 aimOffset;

        // 장전할 수 있는 최대 총알갯수
        public float maxBullet = 8f;
        private float currentBullet;
        public float CurrentAmmoRatio => currentBullet / maxBullet;

        // 발사 간격
        public float bulletChargeDelay = 0.5f;
        // 마지막 발사 시간
        private float lastTimeShoot;

        // Vfx, Sfx
        // 총구 위치
        public Transform weaponMuzzle;
        // 총구 발사 효과
        public GameObject muzzleFlashPrefab;
        public AudioClip shootSfx;

        // 반동
        public float recoilForce = 0.5f;

        // Projectile
        public ProjectileBase projectilePrefab;
        
        public Vector3 MuzzleWorldVelocity { get; private set; }
        private Vector3 lastMuzzlePos;

        // 한번 발사 시 탄환 갯수
        public int bulletsPershot = 1;
        // 총알이 퍼져나가는 각도
        public float bulletSpreadAnlge = 0f;

        // Charge : 발사 버튼을 누르고 있으면 발사체의 대미지, 속도가 일정값 까지 커진다
        public float CurrentCharge { get; private set; }
        public bool IsCharge {  get; private set; }
        // 충전 시작 버튼을 누르기 위해 필요한 ammo량
        public float useCharge = 1f;
        // 차지하고 있는동안 소비되는 ammo 량
        public float rateCharge = 1f;
        // 충전 시간 max
        private float maxChargeDuration = 2f;
        // 층잔ㅅ;짃;긴
        public float lastChargeTriggerTime;

        // Reload
        // 초당 재장전되는 량
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

            // this 무기로 변경
            if(show == true && switchWeaponSfx != null)
            {
                // 무기 변경 효과음
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }

            IsWeaponActive = show;
        }

        // 키 입력에 따른 Shoot 구현
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
                        // 충전 시작
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
                    // 현재 남아있는 충전량
                    float chargeLeft = 1f - CurrentCharge;
                    // 이번 프레임 충전량
                    float chargeAdd = 0f;

                    if(maxChargeDuration <= 0f)
                    {
                        // 한번에 풀충전
                        chargeAdd = chargeLeft;
                    }
                    else
                    {
                        chargeAdd = (1f / maxChargeDuration) * Time.deltaTime;
                    }

                    // 남아있는 충전량보다 작아야 함
                    chargeAdd = Mathf.Clamp(chargeAdd, 0f, chargeLeft);

                    // chargeAdd 만큼 Ammo가 소비
                    float ammoChargeReq = chargeAdd * rateCharge;

                    if(ammoChargeReq <= currentBullet)
                    {
                        UseAmmo(ammoChargeReq);

                        CurrentCharge = Mathf.Clamp01(CurrentCharge + chargeAdd);
                    }
                }
            }

        }
        // 발사 시작
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
        // 발사 끝
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

        // 총 발사 연출
        void HandleShoot()
        {
            for(int i = 0; i < bulletsPershot; i++)
            {
                Vector3 shootdir = GetShootDirection(weaponMuzzle);

                // Projectile 생성
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

        // projectile 날라가는 방향
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