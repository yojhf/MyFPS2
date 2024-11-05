using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �׾��� �� Health�� ���� ������Ʈ�� ų�ϴ� Ŭ����
namespace Unity.FPS.Game
{
    public class Destructable : MonoBehaviour
    {

        Health health;

        private void Start()
        {
            health = GetComponent<Health>();
            DebugUtility.HandleErrorIfNullGetComponent<Health, Destructable>(health, this, gameObject);

            health.OnDie += OnDie;
            health.OnDamaged += OnDamaged;
        }

        void OnDamaged(float damage, GameObject damageSource)
        {

        }

        void OnDie()
        {
            Destroy(gameObject);
        }
    }
}