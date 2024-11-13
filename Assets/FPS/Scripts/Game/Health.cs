using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


// 체력을 관리하는 클래스

namespace Unity.FPS.Game
{
    public class Health : MonoBehaviour
    {
        public float CurrentHealth { get; private set; }
        public bool Invincible { get; private set; }

        [SerializeField] private float maxHealth = 100f;
        // 체력위험 경계
        [SerializeField] private float criticalHealthRatio = 0.3f;

        private bool isDeath = false;

        public UnityAction<float, GameObject> OnDamaged;
        public UnityAction<float> OnHeal;
        public UnityAction OnDie;

        public bool CanPickUpHp() => CurrentHealth < maxHealth;

        public float GetRatio() => CurrentHealth / maxHealth;


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
            CurrentHealth = maxHealth;
            Invincible = false;
        }

        public void TakeDamage(float damage, GameObject damageSource)
        {
            if(Invincible || isDeath)
            {
                return;
            }

            float beforeHealth = CurrentHealth;

            CurrentHealth -= damage;

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            //Debug.Log(CurrentHealth);

            float realDamage = beforeHealth - CurrentHealth;

            if(realDamage > 0)
            {
                if(OnDamaged != null)
                {
                    // 데미지 구현
                    OnDamaged.Invoke(realDamage, damageSource);
                }


            }

            HandleDeath();
        }

        void HandleDeath()
        {
            if (CurrentHealth <= 0)
            {
                isDeath = true;

                if (OnDie != null)
                {
                    OnDie.Invoke();
                    Debug.Log("Die");
                }    
            }
        }

        public void Heal(float heal)
        {
            if (!isDeath)
            {
                return;
            }

            float beforeHealth = CurrentHealth;

            CurrentHealth += heal;

            CurrentHealth = Mathf.Clamp(CurrentHealth, 0, maxHealth);

            float realHeal = CurrentHealth - beforeHealth;

            if (realHeal > 0)
            {
                if (OnHeal != null)
                {
                    // 데미지 구현
                    OnHeal.Invoke(heal);
                }
            }

        }

        public bool IsCritical()
        {
            if (GetRatio() == criticalHealthRatio)
            {
                return true;
            }

            return false;
        }

    }

}
