using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 

// 발사체의 기본이 되는 부모 클래스
namespace Unity.FPS.Game
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        // 발사한 주체
        public GameObject Owner { get; private set; }
        public Vector3 InitPos { get; private set; }
        public Vector3 InitDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }
        public float InitCharge { get; private set; }

        // 발사 시 등록된 함수 호출
        public UnityAction OnShoot;

        public void Shoot(WeaponCon weaponCon)
        {
            Owner = weaponCon.Owner;
            InitPos = transform.position;
            InitDirection = transform.forward;
            InheritedMuzzleVelocity = weaponCon.MuzzleWorldVelocity;
            InitCharge = weaponCon.CurrentCharge;

            OnShoot?.Invoke();
        }

    }
}