using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ������ �����ϴ� Ŭ����
namespace Unity.FPS.Game
{
    public class WeaponCon : MonoBehaviour
    {
        // ���� Ȱ��ȭ, ��Ȱ��ȭ
        private GameObject weaponRoot;

        // ���� ����
        public GameObject Owner { get; set; }
        // ���⸦ ������ �������� ������
        public GameObject SourcePrefab { get; set; }
        // ���� Ȱ��ȭ ����
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

            // this ����� ����
            if(show == true && switchWeaponSfx != null)
            {
                // ���� ���� ȿ����
                shootAudioSource.PlayOneShot(switchWeaponSfx);
            }

            IsWeaponActive = show;
        }
    }
}