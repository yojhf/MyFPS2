using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private GameObject explorsionEffect;
        [SerializeField] private Transform effectPos;
        [SerializeField] private GameObject enemy;

        MaterialPropertyBlock propertyBlock;
        Renderer enemyRenderer;
        Health health;

        Color startColor;

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
            //enemyRenderer = enemy.GetComponent<Renderer>();
            //propertyBlock = new MaterialPropertyBlock();
            //startColor = enemy.GetComponent<Renderer>().material.color;

            health.OnDamaged += OnDamaged;
            health.OnDie += OnDie;

        }

        void OnDamaged(float damage, GameObject damageSource)
        {

        }
        void OnDie()
        {
            GameObject deathEffect = Instantiate(explorsionEffect, effectPos.position, Quaternion.identity);

            Destroy(deathEffect, 5f);
        }
    }
}