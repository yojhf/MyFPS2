using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

// ���� Ammo ī��Ʈ
namespace Unity.FPS.UI
{
    public class AmmoCount : MonoBehaviour
    {
        [SerializeField] private TMP_Text weaponIndexText;
        [SerializeField] private Image ammoFillIamge;
        // ������ ä��� �ӵ�
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float ammoFillSharp = 10f;
        [SerializeField] private float weaponSwitchSharp = 10f;      
        [SerializeField] [Range(0, 1)] private float unSelectedOpa = 0.5f;
        private Vector3 unSelectedScale = Vector3.one * 0.8f;

        private int weaponIndex;


        // �������� ������
        public FillBarColor fillBarColor;

        WeaponCon weaponCon;
        PlayerWeaponsManager playerWeaponsManager;

        // Update is called once per frame
        void Update()
        {
            UpdateAmmoFill();
            ActiveWeapon();
        }

        public void Init(WeaponCon weapon, int _weaponIndex)
        {
            weaponCon = weapon;
            weaponIndex = _weaponIndex;

            weaponIndexText.text = (weaponIndex + 1).ToString();

            // ������ �� �� �ʱ�ȭ
            fillBarColor.Init(1f, 0.1f);

            playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();
        }

        void UpdateAmmoFill()
        {
            float currentFillRate = weaponCon.CurrentAmmoRatio;

            ammoFillIamge.fillAmount = Mathf.Lerp(ammoFillIamge.fillAmount, currentFillRate, ammoFillSharp * Time.deltaTime);

            fillBarColor.UpdateVisual(currentFillRate);
        }

        // ��Ƽ�� ���� ����
        void ActiveWeapon()
        {
            bool isActiveWeapon = (weaponCon == playerWeaponsManager.GetActiveWeapon());
            float currentOpa = isActiveWeapon ? 1.0f : unSelectedOpa;
            Vector3 currentScale = isActiveWeapon ? Vector3.one : unSelectedScale;
            
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, currentOpa, weaponSwitchSharp * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, currentScale, weaponSwitchSharp * Time.deltaTime);
        }
    }
}