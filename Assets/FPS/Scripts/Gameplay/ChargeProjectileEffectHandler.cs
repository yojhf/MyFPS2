using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

// 충전용 발사체를 발사할 때 충전량에 발사체의 게임 오브젝트 크기 결정
namespace Unity.FPS.Gameplay
{
    public class ChargeProjectileEffectHandler : MonoBehaviour
    {
        public GameObject chargeObject;
        public MinMaxVector3 scale;

        ProjectileBase projectileBase;

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        void OnShoot()
        {
            chargeObject.transform.localScale = scale.GetValueFromRatio(projectileBase.InitCharge);
        }
    }
}