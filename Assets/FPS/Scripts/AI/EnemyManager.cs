using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy ����Ʈ �����ϴ� Ŭ����
namespace Unity.FPS.AI
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyController> Enemies { get; private set; }
        // �� ����� enemy ���� ��
        public int EnemyTotalNum { get; private set; }
        // ���� ����ִ�
        public int EnemyRemainingNum => Enemies.Count;

        private void Awake()
        {
            Enemies = new List<EnemyController>();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        // ���
        public void RegisterEnemy(EnemyController newEmeny)
        {
            Enemies.Add(newEmeny);
            EnemyTotalNum++;
        }

        // ����
        public void RemoveEnemy(EnemyController emeny)
        {
            Enemies.Remove(emeny);
        }
    }
}