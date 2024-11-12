using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.Gameplay
{
    public class ChargeWeaponEffectHandler : MonoBehaviour
    {
        // 충전하는 발사체
        public GameObject chargingObject;
        // 발사체를 감싸고 있는 회전하는 프레임
        public GameObject spiningFrame;
        // 발사체를 감싸고 있는 회전하는 이펙트
        public GameObject distOrbitParticePrefab;

        // 발사체의 크기 설정값
        public MinMaxVector3 scale;

        public Vector3 offset;
        public Transform parentTransform;

        // 이팩트 설정값
        public MinMaxFloat orbitY;
        public MinMaxVector3 radius;
        
        // 회전 설정값
        public MinMaxFloat spiningSpeed;

        public GameObject particle { get; private set; }
        ParticleSystem diskOrbit;
        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetimeModule;

        // sfx
        public AudioClip chargeSound;
        public AudioClip loopChargeWeapon;

        public bool useProceduralPitchOnLoop;
        private float fadeLoopDuration = 0.5f;

        public float maxProceduralPitchValue = 2f;
        AudioSource audioSource;
        AudioSource audioSourceLoop;

        private float lastChargeTime;
        private float endChargeTime;
        private float chargeRatio;

        WeaponCon weaponCon;

        private void Awake()
        {
            SoundInit();

        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            // 한번만 실행
            if(particle == null)
            {
                SpawnParitcle();
            }

            diskOrbit.gameObject.SetActive(weaponCon.IsWeaponActive);
            chargeRatio = weaponCon.CurrentCharge;

            UpdateDiskEffect();
            UpdateSoundEffect();
        }

        void SoundInit()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = chargeSound;
            audioSource.playOnAwake = false;

            audioSourceLoop = gameObject.AddComponent<AudioSource>();
            audioSourceLoop.clip = loopChargeWeapon;
            audioSourceLoop.playOnAwake = false;
            audioSourceLoop.loop = true;
        }

        void SpawnParitcle()
        {
            if(parentTransform != null)
            {
                particle = Instantiate(distOrbitParticePrefab, parentTransform);
            }
            else
            {
                particle = Instantiate(distOrbitParticePrefab, transform);
            }

            particle.transform.localPosition += offset;

            FindReference();
        }

        void FindReference()
        {
            weaponCon = GetComponent<WeaponCon>();
            diskOrbit = particle.GetComponent<ParticleSystem>();
            velocityOverLifetimeModule = diskOrbit.velocityOverLifetime;
        }

        void UpdateDiskEffect()
        {
            //disk
            chargingObject.transform.localScale = scale.GetValueFromRatio(chargeRatio);

            if (spiningFrame != null)
            {
                spiningFrame.transform.localRotation *= Quaternion.Euler(0f, spiningSpeed.GetValueFromRatio(chargeRatio) * Time.deltaTime, 0f);
            }

            // particle
            velocityOverLifetimeModule.orbitalY = orbitY.GetValueFromRatio(chargeRatio);
            diskOrbit.transform.localScale = radius.GetValueFromRatio(chargeRatio);

        }

        void UpdateSoundEffect()
        {
            if (chargeRatio > 0f)
            {
                if (audioSourceLoop.isPlaying == false && weaponCon.lastChargeTriggerTime > lastChargeTime)
                {
                    lastChargeTime = weaponCon.lastChargeTriggerTime;

                    if(useProceduralPitchOnLoop == false)
                    {
                        endChargeTime = Time.time + chargeSound.length;
                        audioSource.Play();
                    }

                    audioSourceLoop.Play();
                }

                // 두개의 사운드 페이드 효과로 충전
                if (useProceduralPitchOnLoop == false)
                {
                    float volumeRatio = Mathf.Clamp01((endChargeTime - Time.time - fadeLoopDuration) / fadeLoopDuration);

                    audioSource.volume = volumeRatio;
                    audioSourceLoop.volume = 1f - volumeRatio;

                }
                else
                {
                    audioSourceLoop.pitch = Mathf.Lerp(1f, maxProceduralPitchValue, chargeRatio);
                }
            }
            else
            {
                audioSource.Stop();
                audioSourceLoop.Stop();
            }

       
        }
    }
}