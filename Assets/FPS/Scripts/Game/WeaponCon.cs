using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// ������ �����ϴ� Ŭ����
namespace Unity.FPS.Game
{
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