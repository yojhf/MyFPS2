using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

// ������ �߻�ü�� �߻��� �� �߻�ü�� �Ӽ� ���� ����
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

        // �߻�ü �߻� �� ProjectileBase�� OnShoot �̺�Ʈ �Լ����� ȣ��
        // �߻��� �Ӽ����� Charge���� ���� ����
        void OnShoot()
        {
            // �������� ���� �߻�ü �Ӽ��� ����
            ProjectileStandard projectileStandard = GetComponent<ProjectileStandard>();

            projectileStandard.damage = Damage.GetValueFromRatio(projectileBase.InitCharge);
            projectileStandard.speed = Speed.GetValueFromRatio(projectileBase.InitCharge);
            projectileStandard.gravityDown = GravityDown.GetValueFromRatio(projectileBase.InitCharge);
            projectileStandard.radius = Radius.GetValueFromRatio(projectileBase.InitCharge);

        }
    }
}