using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ����� ���� ��� ����
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

        // ������ ��ġ�� ���ӿ�����Ʈ �����ϰ� AudioSource ������Ʈ�� �߰��ؼ� ������ Ŭ���� �÷���
        // Ŭ������ �÷��̰� ������ �ڵ����� ����� - TimeSelfDestruct �̿�
        public void CreateSfx(AudioClip clip, Vector3 position, float spartialBlend, float rolloffDistance = 1f)
        {
            GameObject impactSfx = new GameObject();

            impactSfx.transform.position = position;

            AudioSource source = impactSfx.AddComponent<AudioSource>();

            source.clip = clip; 
            source.spatialBlend = spartialBlend;
            source.minDistance = rolloffDistance;
            source.Play();

            // ������Ʈ kill
            TimeSelfDestruct timeSelf = impactSfx.AddComponent<TimeSelfDestruct>();

            timeSelf.killTime = clip.length;


        }
    }
}