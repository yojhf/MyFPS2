using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class WorldSpaceHealthBar : MonoBehaviour
    {
        public Transform pivot;
        public Image hpBar;
        public float speed = 5f;

        public bool hideHealthBar = true;
        Health health;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHpBarUI();
        }

        void Init()
        {
            health = GetComponent<Health>();
        }

        void UpdateHpBarUI()
        {
            float curHp = health.GetRatio();

            hpBar.fillAmount = Mathf.Lerp(hpBar.fillAmount, curHp, Time.deltaTime * 10);

            pivot.LookAt(Camera.main.transform.position);

            if(hideHealthBar)
            {
                if (health.GetRatio() == 1)
                {
                    pivot.gameObject.SetActive(false);
                }
                else
                {
                    pivot.gameObject.SetActive(true);
                }
            }

        }
    }
}