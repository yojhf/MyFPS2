using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WeaponHUDManager : MonoBehaviour
    {
        // ammoCountUI 부모 오브젝트
        public RectTransform ammoPanel;
        // ammoCountUI프리펩
        public GameObject ammoCountPrefab;

        PlayerWeaponsManager playerWeaponsManager;

        // Start is called before the first frame update
        void Awake()
        {
            playerWeaponsManager = FindObjectOfType<PlayerWeaponsManager>();



            playerWeaponsManager.OnAddedWeapon += AddWeapon;
            playerWeaponsManager.OnRemoveWeapon += RemoveWeapon;
            playerWeaponsManager.OnSwitchToWeapon += SwitchWeapon;
        }

        // Update is called once per frame
        void Update()
        {

        }

        void AddWeapon(WeaponCon newWeapon, int weaponIndex)
        {
            GameObject ammoCount = Instantiate(ammoCountPrefab, ammoPanel);

            ammoCount.GetComponent<AmmoCount>().Init(newWeapon, weaponIndex);
        }

        void RemoveWeapon(WeaponCon newWeapon, int weaponIndex)
        {
            
        }

        void SwitchWeapon(WeaponCon weapon)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(ammoPanel);
        }
    }
}