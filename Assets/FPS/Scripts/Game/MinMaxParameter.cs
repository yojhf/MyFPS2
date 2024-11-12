using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Unity.FPS.Game
{
    // ratio �Ű������� �޾� float�� Min���� Max ������ ���� Lerp ��ȯ
    [Serializable]
    public class MinMaxFloat
    {
        public float Min;
        public float Max;

        public float GetValueFromRatio(float ratio)
        {
            return Mathf.Lerp(Min, Max, ratio);
        }
    }

    [Serializable]
    public class MinMaxColor
    {
        public Color Min;
        public Color Max;

        public Color GetValueFromRatio(float ratio)
        {
            return Color.Lerp(Min, Max, ratio);
        }
    }

    [Serializable]
    public class MinMaxVector3
    {
        public Vector3 Min;
        public Vector3 Max;

        public Vector3 GetValueFromRatio(float ratio)
        {
            return Vector3.Lerp(Min, Max, ratio);
        }
    }

}