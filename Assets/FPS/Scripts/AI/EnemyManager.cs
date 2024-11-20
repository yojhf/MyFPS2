using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Enemy 리스트 관리하는 클래스
namespace Unity.FPS.AI
{
    public class EnemyManager : MonoBehaviour
    {
        public List<EnemyController> Enemies { get; private set; }
        // 총 생산된 enemy 수의 합
        public int EnemyTotalNum { get; private set; }
        // 현재 살아있는
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

        // 등록
        public void RegisterEnemy(EnemyController newEmeny)
        {
            Enemies.Add(newEmeny);
            EnemyTotalNum++;
        }

        // 제거
        public void RemoveEnemy(EnemyController emeny)
        {
            Enemies.Remove(emeny);
        }
    }
}