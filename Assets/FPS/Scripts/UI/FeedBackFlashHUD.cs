using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;


namespace Unity.FPS.UI
{
    public class FeedBackFlashHUD : MonoBehaviour
    {
        public Image flashImage;
        public Color damageFlashColor;
        public Color healFlashColor;

        public float flashDuration = 1f;
        public float flashMaxAlpha = 1f;

        private bool flashActive = false;
        private float lastTime = Mathf.NegativeInfinity;

        CanvasGroup canvasGroup;
        Health health;

        // Start is called before the first frame update
        void Start()
        {
            canvasGroup = flashImage.GetComponent<CanvasGroup>();

            health = FindObjectOfType<CharacterController>().GetComponent<Health>();

            health.OnDamaged += OnDamaged;
            health.OnHeal += OnHeal;
        }

        // Update is called once per frame
        void Update()
        {
            if (flashActive)
            { 
                float nomalizedTime = (Time.time - lastTime) / flashDuration;

                if (nomalizedTime < 1f)
                {
                    float flashAmount = flashMaxAlpha * (1 - nomalizedTime);

                    canvasGroup.alpha = flashAmount;
                }
                else
                {
                    canvasGroup.gameObject.SetActive(false);
                    flashActive = false;
                }

            }
        }

        // �ʱ�ȭ
        void ResetFlash()
        {
            flashActive = true;

            // ȿ�� ���� �ð�
            lastTime = Time.time;
            canvasGroup.alpha = 0f;
            canvasGroup.gameObject.SetActive(true);

        }

        // ������ ���� �� ������ �÷��� ����
        void OnDamaged(float damage, GameObject damageSource)
        {
            ResetFlash();

            flashImage.color = damageFlashColor;
        }

        void OnHeal(float damage)
        {
            ResetFlash();

            flashImage.color = healFlashColor;
        }
    }
}