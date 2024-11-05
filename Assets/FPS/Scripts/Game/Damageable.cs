using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// �浹ü(hit box)�� �����Ǿ� �������� �����ϴ� Ŭ����
namespace Unity.FPS.Game
{
    public class Damageable : MonoBehaviour
    {
        // ������ ���
        [SerializeField] private float damageMultiplier = 1f;
        // �ڽ��� ���� ������ ���
        [SerializeField] private float sensiblityToSelfDamage = 0.5f;


        Health health;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Init()
        {
            health = GetComponent<Health>();

            if (health == null)
            {
                health = GetComponentInParent<Health>();
            }
        }

        public void InflictDamage(float damage, bool isExplosionDamage, GameObject damageSource)
        {
            if (health == null)
            {
                return;
            }

            // totalDamage = ���� ������ ��
            var totalDamage = damage;

            // ���� ������ üũ - ���� �������϶��� damageMultiplier�� ������� �ʴ´�
            if(isExplosionDamage == false)
            {
                totalDamage *= damageMultiplier;
            }

            // �ڽ��� ���� ���̹��� �̸�
            if(health.gameObject == damageSource)
            {
                totalDamage *= sensiblityToSelfDamage;
            }

            // ������ ������
            health.TakeDamage(totalDamage, damageSource);
        }



    }
}
