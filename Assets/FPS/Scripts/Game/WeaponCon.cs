using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 무기을 관리하는 클래스
namespace Unity.FPS.Game
{
    public class WeaponCon : MonoBehaviour
    {
        // 무기 활성화, 비활성화
        private GameObject weaponRoot;

        // 무기 주인
        public GameObject Owner { get; set; }
        // 무기를 생성한 오리지널 프리펩
        public GameObject SourcePrefab { get; set; }
        // 무기 활성화 여부
        public bool IsWeaponActive { get; private set; }

        public AudioClip switchWeaponSfx;
        AudioSource shootAudioSource;

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
    }
}