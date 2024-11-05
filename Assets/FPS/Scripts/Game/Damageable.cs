using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 충돌체(hit box)에 부착되어 데미지를 관리하는 클래스
namespace Unity.FPS.Game
{
    public class Damageable : MonoBehaviour
    {
        // 데미지 계수
        [SerializeField] private float damageMultiplier = 1f;
        // 자신이 입힌 데미지 계수
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

            // totalDamage = 실제 데미지 값
            var totalDamage = damage;

            // 폭발 데미지 체크 - 폭발 데미지일때는 damageMultiplier를 계산하지 않는다
            if(isExplosionDamage == false)
            {
                totalDamage *= damageMultiplier;
            }

            // 자신이 입힌 데이미지 이면
            if(health.gameObject == damageSource)
            {
                totalDamage *= sensiblityToSelfDamage;
            }

            // 데미지 입히기
            health.TakeDamage(totalDamage, damageSource);
        }



    }
}
