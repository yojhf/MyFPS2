using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

// 충전용 발사체를 발사할 때 발사체의 속성 값을 설정
namespace Unity.FPS.Gameplay
{
    public class ProjectileChargeParameters : MonoBehaviour
    {
        private ProjectileBase projectileBase;

        public MinMaxFloat Damage;
        public MinMaxFloat Speed;
        public MinMaxFloat GravityDown;
        public MinMaxFloat Radius;

        private void OnEnable()
        {
            projectileBase = GetComponent<ProjectileBase>();
            projectileBase.OnShoot += OnShoot;
        }

        // 발사체 발사 시 ProjectileBase의 OnShoot 이벤트 함수에서 호출
        // 발사의 속성값의 Charge값에 따라 설정
        void OnShoot()
        {
            // 충전량에 따라 발사체 속성값 설정
            ProjectileStandard projectileStandard = GetComponent<ProjectileStandard>();

            projectileStandard.damage = Damage.GetValueFromRatio(projectileBase.InitCharge);
            projectileStandard.speed = Speed.GetValueFromRatio(projectileBase.InitCharge);
            projectileStandard.gravityDown = GravityDown.GetValueFromRatio(projectileBase.InitCharge);
            projectileStandard.radius = Radius.GetValueFromRatio(projectileBase.InitCharge);

        }
    }
}