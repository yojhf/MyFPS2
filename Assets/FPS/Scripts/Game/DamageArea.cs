using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 일정 범위 안에 있는 콜라이더의 오브젝트 데미지
namespace Unity.FPS.Game
{ 
    public class DamageArea : MonoBehaviour
    {
        public float areaEffect = 10f;

        public AnimationCurve damageRatio;
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void InflictDamageArea(float damage, Vector3 center, LayerMask layers,
            QueryTriggerInteraction interaction, GameObject owner)
        {
            Dictionary<Health, Damageable> uniqueDamagedHealth = new Dictionary<Health, Damageable>();

            Collider[] colliders = Physics.OverlapSphere(center, areaEffect, layers, interaction);

            foreach (var collider in colliders)
            {
                Damageable damageable = collider.GetComponent<Damageable>();

                if (damageable != null)
                {
                    Health health = collider.GetComponentInParent<Health>();

                    if (health != null && !uniqueDamagedHealth.ContainsKey(health))
                    {
                        uniqueDamagedHealth.Add(health, damageable);
                    }


                }
            }

            // Damage
            foreach (var uniqueDamage in uniqueDamagedHealth.Values)
            {
                float distance = Vector3.Distance(uniqueDamage.transform.position, center);
                float curveDamage = damage * damageRatio.Evaluate(distance);

                uniqueDamage.InflictDamage(curveDamage, true, owner);

                Debug.Log(curveDamage);

            }
        }
    }
}