using System.Collections;
using System.Collections.Generic;
using Unity.FPS.Game;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.FPS.UI
{
    public class PlayerHealthBar : MonoBehaviour
    {
        public Image healFillImage;

        Health health;
        // Start is called before the first frame update
        void Start()
        {
            health = FindObjectOfType<CharacterController>().GetComponent<Health>();
        }

        // Update is called once per frame
        void Update()
        {
            healFillImage.fillAmount = Mathf.Lerp(healFillImage.fillAmount, health.GetRatio(), 3f * Time.deltaTime);
        }
    }
}