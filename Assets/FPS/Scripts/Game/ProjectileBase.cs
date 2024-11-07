using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; 

// �߻�ü�� �⺻�� �Ǵ� �θ� Ŭ����
namespace Unity.FPS.Game
{
    public abstract class ProjectileBase : MonoBehaviour
    {
        // �߻��� ��ü
        public GameObject Owner { get; private set; }
        public Vector3 InitPos { get; private set; }
        public Vector3 InitDirection { get; private set; }
        public Vector3 InheritedMuzzleVelocity { get; private set; }
        public float InitCharge { get; private set; }

        // �߻� �� ��ϵ� �Լ� ȣ��
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