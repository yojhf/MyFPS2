using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 죽었을 때 Health를 가진 오브젝트를 킬하는 클래스
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