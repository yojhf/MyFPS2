using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.FPS.AI
{
    public class PatrolPath : MonoBehaviour
    {
        [SerializeField] private Transform enemiesPar;

        public List<Transform> wayPoints = new List<Transform>(); 

        // Path�� ��Ʈ���ϴ� enemy
        public List<EnemyController> enemies = new List<EnemyController>();

        // ����� path
        public float radius = 1f;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        void Init()
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                wayPoints.Add(transform.GetChild(i));
            }

            for (int i = 0; i < enemiesPar.childCount; i++)
            {
                enemies.Add(enemiesPar.GetChild(i).GetComponent<EnemyController>());
            }

            // ��ϵ� enemy���� ��Ʈ���� �н� ����
            foreach (var enemy in enemies)
            {
                enemy.PatrolPath = this;
            }
        }

        public Vector3 GetPosition(int wayPointIndex)
        {
            if (wayPointIndex < 0 || wayPointIndex >= wayPoints.Count || wayPoints[wayPointIndex] == null)
                return Vector3.zero;

            return wayPoints[wayPointIndex].position;
        }

        // Ư��(enemy) ��ġ�� ���� ������ ������ �Ÿ� ���ϱ� 
        public float GetDistacne(Vector3 origin, int wayPointIndex)
        {
            if(wayPointIndex < 0 || wayPointIndex >= wayPoints.Count || wayPoints[wayPointIndex] == null)
            {
                return -1f;
            }

            return (wayPoints[wayPointIndex].position - origin).magnitude;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;

            for(int i = 0; i < wayPoints.Count; i++)
            {
                int nextIndex = i + 1;

                if(nextIndex >= wayPoints.Count)
                {
                    nextIndex -= wayPoints.Count;
                }
                
                Gizmos.DrawLine(wayPoints[i].position, wayPoints[nextIndex].position);
                Gizmos.DrawSphere(wayPoints[i].position, radius);
            }
        }
    }
}