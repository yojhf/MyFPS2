using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
        public float CurrentCharge { get; private set; }



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

        }

        void Init()
        {
            weaponRoot = transform.GetChild(0).gameObject;
            shootAudioSource = GetComponent<AudioSource>();
            currentBullet = maxBullet;
            lastTimeShoot = Time.time;
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
                    if (inputUp == true)
                    {
                        return TryShoot();            
                    }
                    break;

            }

            return false;
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
    }
}