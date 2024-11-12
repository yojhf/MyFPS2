using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;

// ������ �߻�ü�� �߻��� �� �������� �߻�ü�� ���� ������Ʈ ũ�� ����
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