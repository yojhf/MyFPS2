using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 오디오 관련 기능 구현
namespace Unity.FPS.Game
{
    public class AudioUtility : MonoBehaviour
    {
        public static AudioUtility instacne;
        // Start is called before the first frame update

        private void Awake()
        {
            instacne = this;
        }

        // 지정된 위치에 게임오브젝트 생성하고 AudioSource 컴포넌트를 추가해서 지정된 클립을 플레이
        // 클립사운드 플레이가 끝나면 자동으로 사라짐 - TimeSelfDestruct 이용
        public void CreateSfx(AudioClip clip, Vector3 position, float spartialBlend, float rolloffDistance = 1f)
        {
            GameObject impactSfx = new GameObject();

            impactSfx.transform.position = position;

            AudioSource source = impactSfx.AddComponent<AudioSource>();

            source.clip = clip; 
            source.spatialBlend = spartialBlend;
            source.minDistance = rolloffDistance;
            source.Play();

            // 오브젝트 kill
            TimeSelfDestruct timeSelf = impactSfx.AddComponent<TimeSelfDestruct>();

            timeSelf.killTime = clip.length;


        }
    }
}