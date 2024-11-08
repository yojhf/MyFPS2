using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 게이지바의 색, 백그라운드 색 변경
namespace Unity.FPS.UI
{
    public class FillBarColor : MonoBehaviour
    {
        public Image fgImage;
        // 게이지가 풀로 차는 순간 플래시
        public Color flashFgColor;
        public Color defaultFgColor;

        public Image bgImage;
        public Color defaultBgColor;
        public Color flashBgColor;

        private float fullValue = 1f;
        private float emptyValue = 0f;

        private float colorChangeSharp = 5f;
        // 게이지가 풀로 차는 순간
        private float prevousValue;

        public void Init(float fullValueRatio, float emptyValueRatio)
        {
            fullValue = fullValueRatio;
            emptyValue = emptyValueRatio;

            prevousValue = fullValue;
        }


        public void UpdateVisual(float currentRatio)
        {
            if(currentRatio == fullValue && currentRatio != prevousValue)
            {
                fgImage.color = flashFgColor;

            }
            else if(currentRatio < emptyValue)
            {
                bgImage.color = flashBgColor;
            }
            else
            {
                fgImage.color = Color.Lerp(fgImage.color, defaultFgColor, colorChangeSharp * Time.deltaTime);
                bgImage.color = Color.Lerp(bgImage.color, defaultBgColor, colorChangeSharp * Time.deltaTime);
            }

            prevousValue = currentRatio;
        }
    }
}