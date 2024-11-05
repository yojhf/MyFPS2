using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.UIElements;
using UnityEngine.Events;


// �÷��̾ ���� ������� �����ϴ� Ŭ����
namespace Unity.FPS.Gameplay
{
    public enum WeaponSwitchState
    {
        Up,
        Down,
        PutDownPrvious,
        PutUpNew

    }

    public class PlayerWeaponsManager : MonoBehaviour
    {
        // ���� ���� - ������ ������ �� ó�� �������� ���޵Ǵ� ���� ����Ʈ
        public List<WeaponCon> startingWeapons = new List<WeaponCon>();

        // ���� ����

        // ���⸦ �����ϴ� ������Ʈ 
        public Transform weaponParentSocket;

        // �÷��̾ �����߿� ��� �ٴϴ� ���� ����Ʈ
        private WeaponCon[] weaponSlots = new WeaponCon[9];

        // ���� ����Ʈ�� �����ϴ� �ε���
        public int ActiveWeaponIndex {  get; private set; }

        // ���� ��ü - ��ϵ� �Լ� ȣ��
        public UnityAction<WeaponCon> OnSwitchToWeapon;

        // ���� ��ü �� ����
        private WeaponSwitchState weaponSwitchState;

        // ���� ��ü �� ���Ǵ� ���� ��ġ
        private Vector3 weaPonMainLocalPos;
        public Transform defaultWeaponPos;
        public Transform downWeaponPos;
        // ���� �ٲ�� ���� �ε���
        private int weaponSwitchNewIndex;
        private float weaponSwitchTimeStarted = 0f;
        public float weaponSwitchDelay = 1f;



        PlayerInputHandler playerInputHandler;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            SwitchWeaponInput();
        }

        private void LateUpdate()
        {
            UpdateWeaponSwitching();

            // ���� ���� ��ġ
            weaponParentSocket.localPosition = weaPonMainLocalPos;
        }

        void Init()
        {
            // �ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;
            playerInputHandler = GetComponent<PlayerInputHandler>();

            OnSwitchToWeapon += OnWeaponSwitched;

            // 
            foreach (var weaponSlot in startingWeapons)
            {
                AddWeapon(weaponSlot);
            }

            SwitchWeapon(true);
        }

        // ���¿� ���� ���� ����
        void UpdateWeaponSwitching()
        {
            // Lerp
            float switchingTimeFactor = 0f;

            if(weaponSwitchDelay == 0f)
            {
                switchingTimeFactor = 1f;
            }
            else
            {
                switchingTimeFactor = Mathf.Clamp01((Time.time - weaponSwitchTimeStarted) / weaponSwitchDelay);
            }

            // �����ð� ���� ���� ���� �ٲٱ�
            if (switchingTimeFactor >= 1f)
            {
                if(weaponSwitchState == WeaponSwitchState.PutDownPrvious)
                {
                    // ���� ���� false, ���ο� ���� true
                    WeaponCon oldWeapon = GetActiveWeapon();

                    if (oldWeapon != null)
                    {
                        oldWeapon.ShowWeapon(false);
                    }

                    ActiveWeaponIndex = weaponSwitchNewIndex;

                    WeaponCon newWeapon = GetActiveWeapon();

                    OnSwitchToWeapon?.Invoke(newWeapon);

                    switchingTimeFactor = 0f;

                    if (newWeapon != null)
                    {
                        weaponSwitchTimeStarted = Time.time;
                        weaponSwitchState = WeaponSwitchState.PutUpNew;
                    }
                    else
                    {
                        weaponSwitchState = WeaponSwitchState.Down;
                    }

                }
                else if(weaponSwitchState == WeaponSwitchState.PutUpNew)
                {
                    weaponSwitchState = WeaponSwitchState.Up;
                }
            }

            // �����ð����� ������ ��ġ �̵�
            if (weaponSwitchState == WeaponSwitchState.PutDownPrvious)
            {
                weaPonMainLocalPos = Vector3.Lerp(defaultWeaponPos.localPosition, downWeaponPos.localPosition, switchingTimeFactor);
            }
            else if (weaponSwitchState == WeaponSwitchState.PutUpNew)
            {
                weaPonMainLocalPos = Vector3.Lerp(downWeaponPos.localPosition, defaultWeaponPos.localPosition, switchingTimeFactor);
            }


        }



        void SwitchWeaponInput()
        {
            if (weaponSwitchState == WeaponSwitchState.Up || weaponSwitchState == WeaponSwitchState.Down)
            {
                int switchWeaponInput = playerInputHandler.GetSwitchWeaponInput();

                if (switchWeaponInput != 0)
                {
                    bool isSwitchUp = switchWeaponInput > 0;

                    SwitchWeapon(isSwitchUp);
                }
            }
        }

        // weaponSlots�� ���� ���������� ������ WeaponCon ������Ʈ �߰�
        public bool AddWeapon(WeaponCon weaponPrefab)
        {
            // �߰��ϴ� ���� ���� ���� üũ - �ߺ��˻�
            if (HasWeapon(weaponPrefab) != null)
            {
                Debug.Log("Same Weapon");
                return false;
            }

            for(int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] == null)
                {
                    WeaponCon weaponInstance = Instantiate(weaponPrefab, weaponParentSocket);

                    weaponInstance.transform.localPosition = Vector3.zero;
                    weaponInstance.transform.localRotation = Quaternion.identity;

                    weaponInstance.Owner = gameObject;
                    weaponInstance.SourcePrefab = weaponPrefab.gameObject;
                    weaponInstance.ShowWeapon(false);

                    weaponSlots[i] = weaponInstance;

                    return true;
                }
            }

            Debug.Log("Full");

            return false;
        }

        // �Ű������� ���� ���Ⱑ ������ �ִ��� Ȯ��
        WeaponCon HasWeapon(WeaponCon weaponPrefab)
        {
            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if (weaponSlots[i] != null && weaponSlots[i].SourcePrefab == weaponPrefab)
                {
                    return weaponSlots[i];
                }
            }

            return null;
        }

        // 0 ~ 9 / 0, 1, 2,...
        // ���� �ٲٱ�, ���� ��� �ִ� ���� false, ���ο� ���� true
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1; // ���� ��Ƽ���� ���� �ε���
            int closeSlotDistance = weaponSlots.Length;

            for (int i = 0; i < weaponSlots.Length; i++)
            {
                if(i != ActiveWeaponIndex && GetWeaponAtSlotIndex(i) != null)
                {
                    int distanceToActiveIndex = GetDistanceBetweenWeaponSlot(ActiveWeaponIndex, i, ascendingOrder);

                    if(distanceToActiveIndex < closeSlotDistance)
                    {
                        closeSlotDistance = distanceToActiveIndex;

                        newWeaponIndex = i;
                    }
                }
            }

            // ���� ��Ƽ���� ���� �ε����� ���� ��ü
            SwitchToWeaponIndex(newWeaponIndex);

        }

        void SwitchToWeaponIndex(int newWeaponIndex)
        {
            // newWeaponIndex �� üũ
            if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;

                weaponSwitchTimeStarted = Time.time;

                // ���� ��Ƽ���� ���Ⱑ �ִ���
                if(GetActiveWeapon() == null)
                {
                    weaPonMainLocalPos = downWeaponPos.position;
                    weaponSwitchState = WeaponSwitchState.PutUpNew;
                    ActiveWeaponIndex = newWeaponIndex;

                    WeaponCon newWeapon = GetWeaponAtSlotIndex(newWeaponIndex);
                    OnSwitchToWeapon?.Invoke(newWeapon);
                }
                else
                {
                    weaponSwitchState = WeaponSwitchState.PutDownPrvious;
                }
            }



            //if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            //{
            //    if (ActiveWeaponIndex >= 0)
            //    {
            //        WeaponCon nowWeapon = GetWeaponAtSlotIndex(ActiveWeaponIndex);
            //        nowWeapon.ShowWeapon(false);
            //    }

            //    WeaponCon newWeapon = GetWeaponAtSlotIndex(newWeaponIndex);
            //    newWeapon.ShowWeapon(true);

            //    ActiveWeaponIndex = newWeaponIndex;
            //}
        }

        public WeaponCon GetActiveWeapon()
        {
            return GetWeaponAtSlotIndex(ActiveWeaponIndex);
        }

        // ������ ���Կ� ���Ⱑ �ִ��� ����
        public WeaponCon GetWeaponAtSlotIndex(int index)
        {
            if(index >= 0 && index < weaponSlots.Length)
            {
                return weaponSlots[index];
            }

            return null;
        }

        int GetDistanceBetweenWeaponSlot(int fromSlotIndex, int toSlotIndex, bool ascendingOrder)
        {
            int distanceBetweenSlots = 0;

            if(ascendingOrder)
            {
                distanceBetweenSlots = toSlotIndex - fromSlotIndex;
            }
            else
            {
                distanceBetweenSlots = fromSlotIndex - toSlotIndex;
            }

            if(distanceBetweenSlots < 0)
            {
                distanceBetweenSlots = distanceBetweenSlots + weaponSlots.Length;
            }

            return distanceBetweenSlots;
        }

        void OnWeaponSwitched(WeaponCon newWeapon)
        {
            if(newWeapon != null)
            {
                newWeapon.ShowWeapon(true);
            }
        }

    }
}