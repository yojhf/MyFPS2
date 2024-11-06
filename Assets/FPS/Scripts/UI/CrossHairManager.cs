using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.FPS.Gameplay;
using Unity.FPS.Game;

namespace Unity.FPS.UI
{
    public class CrossHairManager : MonoBehaviour
    {
        [SerializeField] private Transform player;
        public Image crossHairImage;
        public Sprite nullcrossHair;
        private RectTransform crossHair;
        private CrossHairData crossHairDefault;
        private CrossHairData crossHairTarget;
        private CrossHairData crossHairCur;
        [SerializeField] private float updateShrpness = 5f;

        private bool wasPointEnemy;

        PlayerWeaponsManager playerWeaponsManager;
        WeaponCon newWeaponCrossHair;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            //ChangeCrossHair();
            UpdateCrossHair(false);

            wasPointEnemy = playerWeaponsManager.IsPointEnemy;
        }

        void Init()
        {
            playerWeaponsManager = player.GetComponent<PlayerWeaponsManager>();
            newWeaponCrossHair = playerWeaponsManager.GetActiveWeapon();

            OnChangeCrossHair(newWeaponCrossHair);

            playerWeaponsManager.OnSwitchToWeapon += OnChangeCrossHair;
        }

        // 크로스헤어 그리기
        void UpdateCrossHair(bool force)
        {
            if (crossHairDefault.crossHairSprit == null)
                return;

            if(/*(force || wasPointEnemy == false) && */playerWeaponsManager.IsPointEnemy == true)
            {
                // 타겟팅
                crossHairCur = crossHairTarget;

            }
            else if (/*(force || wasPointEnemy == true) && */playerWeaponsManager.IsPointEnemy == false)
            {
                // 타겟팅이 안될 때 
                crossHairCur = crossHairDefault;
            }

            //crossHair.sizeDelta = crossHairCur.crossHairSize * Vector2.one;
            crossHairImage.sprite = crossHairCur.crossHairSprit;
            crossHairImage.color = Color.Lerp(crossHairImage.color, crossHairCur.crossHairColor, updateShrpness * Time.deltaTime);
            crossHair.sizeDelta = Mathf.Lerp(crossHair.sizeDelta.x, crossHairCur.crossHairSize, updateShrpness * Time.deltaTime) * Vector2.one;
        }

        // 무기가 바뀔때마다 crossHairImage를 각각의 무기 CrossHair로 바꾸기
        void OnChangeCrossHair(WeaponCon newWeapon)
        {
            if(newWeapon != null)
            {
                crossHairImage.enabled = true;
                crossHair = crossHairImage.GetComponent<RectTransform>();
                crossHairDefault = newWeapon.crossHairDefault;
                crossHairTarget = newWeapon.crossHairTarget;


                //float size = newWeapon.crossHairDefault.crossHairSize / 10;

                //crossHairImage.sprite = newWeapon.crossHairDefault.crossHairSprit;
                //crossHairImage.color = newWeapon.crossHairDefault.crossHairColor;
                //crossHairImage.transform.localScale = new Vector3(size, size, size);
            }
            else
            {
                if(nullcrossHair)
                {
                    crossHairImage.sprite = nullcrossHair;
                }
                else
                {
                    crossHairImage.enabled = false;
                }

            }

            //UpdateCrossHair(true);
        }

        void ChangeCrossHair()
        {
            float size = playerWeaponsManager.GetActiveWeapon().crossHairDefault.crossHairSize / 10;

            crossHairImage.sprite = playerWeaponsManager.GetActiveWeapon().crossHairDefault.crossHairSprit;
            crossHairImage.color = playerWeaponsManager.GetActiveWeapon().crossHairDefault.crossHairColor;
            crossHairImage.transform.localScale = new Vector3(size, size, size);
        }


    }
}