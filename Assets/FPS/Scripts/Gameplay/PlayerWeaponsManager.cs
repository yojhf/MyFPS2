using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEditor.ShaderGraph.Internal;


// �÷��̾ ���� ����(WeaponCon)���� �����ϴ� Ŭ����
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
        public Transform aimingWeaponPos;
        // ���� �ٲ�� ���� �ε���
        private int weaponSwitchNewIndex;
        private float weaponSwitchTimeStarted = 0f;
        public float weaponSwitchDelay = 1f;

        // �� ����
        public bool IsPointEnemy { get; private set; }
        public Camera weaponCam;

        // ����
        // ī�޶� ����
        // �⺻ Fov ��
        public float defaultFov = 60f;
        // Fov ������
        public float weaponFovMult = 1f;
        // ���� ����
        public bool IsAiming { get; private set; }
        // ���� �̵�, Fov ���� �ӵ�
        public float aimingAniSpeed = 10f;

        // ��鸲
        public float bobFrequency = 10f;
        public float bobShrpness = 10f;
        // �⺻ ��鸲
        public float defaultBobAmount = 0.05f;
        // ���� ��鸲
        public float aimingBobAmount = 0.02f;
        // ��鸲 ���
        private float weaponBobFactor;
        // ���� �����ӿ��� �̵��ӵ��� ���
        private Vector3 lastCharPos;
        // ��鸲 �� ���� ��갪, �̵����� ������ 0
        private Vector3 weaponBobLocalPos;


        PlayerCharacterController playerCharacterController;
        PlayerInputHandler playerInputHandler;

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            WeaponCon activeWeapon = GetActiveWeapon();

            // ���� �Է°� ó��
            IsAiming = playerInputHandler.GetAimInputHeld();

            if(!IsAiming)
            {
                SwitchWeaponInput();
            }

            ActiveWeapon(activeWeapon);

        }

        private void LateUpdate()
        {
            UpdateWeaponBob();
            UpdateWeapomAiming();
            UpdateWeaponSwitching();

            // ���� ���� ��ġ
            weaponParentSocket.localPosition = weaPonMainLocalPos + weaponBobLocalPos;
        }

        void Init()
        {
            // �ʱ�ȭ
            ActiveWeaponIndex = -1;
            weaponSwitchState = WeaponSwitchState.Down;
            playerInputHandler = GetComponent<PlayerInputHandler>();
            playerCharacterController = GetComponent<PlayerCharacterController>();

            OnSwitchToWeapon += OnWeaponSwitched;

            SetFov(defaultFov);

            // 
            foreach (var weaponSlot in startingWeapons)
            {
                AddWeapon(weaponSlot);
            }

            SwitchWeapon(true);
        }

        void ActiveWeapon(WeaponCon activeWeapon)
        {
            // �� ����
            IsPointEnemy = false;

            if (activeWeapon)
            {
                RaycastHit hit;

                if (Physics.Raycast(weaponCam.transform.position, weaponCam.transform.forward, out hit, 300f))
                {
                    //Health enemyHealth = hit.collider.GetComponent<Health>();

                    //if(enemyHealth != null)
                    //{
                    //    IsPointEnemy = true;
                    //}


                    if (hit.collider.tag == "Enemy")
                    {
                        IsPointEnemy = true;
                    }
                }
            }
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

        // ī�޶� Fov �� ���� : ����, �ܾƿ�
        void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCam.fieldOfView = fov * weaponFovMult;
        }

        // ���� ���ؿ� ���� ����, ���� ��ġ ����, Fov �� ����
        void UpdateWeapomAiming()
        {
            // ���⸦ ��� �������� ���� ����
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                WeaponCon activeWeapon = GetActiveWeapon();

                //if (activeWeapon == null)
                //    return;

                // ���ؽ� : Default -> Aiming ��ġ�� �̵�
                if (IsAiming && activeWeapon)
                {
                    // ����
                    weaPonMainLocalPos = Vector3.Lerp(weaPonMainLocalPos, aimingWeaponPos.localPosition + activeWeapon.aimOffset, aimingAniSpeed * Time.deltaTime);

                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView, activeWeapon.aimZoomRatio * defaultFov, aimingAniSpeed * Time.deltaTime);
                    
                    SetFov(fov);
                }
                else
                {
                    // ���� ����
                    weaPonMainLocalPos = Vector3.Lerp(weaPonMainLocalPos, defaultWeaponPos.localPosition - activeWeapon.aimOffset, aimingAniSpeed * Time.deltaTime);

                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView, defaultFov, aimingAniSpeed * Time.deltaTime);


                    SetFov(fov);
                }
            }   
        }

        // �̵��� ���� ���� ��鸲 ��
        void UpdateWeaponBob()
        {
            if(Time.deltaTime > 0)
            {
                // �÷��̾ �� �����ӵ��� �̵��� �Ÿ�
                Vector3 distance = playerCharacterController.transform.position - lastCharPos;
                // ���� �����ӿ��� �÷��̾� �̵� �ӵ�
                Vector3 playerVelocity = distance / Time.deltaTime;
                float movementFactor = 0f;
                // ��鸲�� (�⺻, ����)       
                float bobAmount;
                float frequency = bobFrequency;

                if (playerCharacterController.IsGrounded)
                {
                    movementFactor = Mathf.Clamp01(playerVelocity.magnitude / (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                // �ӵ��� ���� ��鸲 ���
                weaponBobFactor = Mathf.Lerp(weaponBobFactor, movementFactor, bobShrpness * Time.deltaTime);

                if(IsAiming)
                {
                    bobAmount = aimingBobAmount;
                }
                else
                {
                    bobAmount = defaultBobAmount;
                }

                // �¿� ��鸲
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f) * bobAmount * weaponBobFactor;


                weaponBobLocalPos.x = hBobValue;
                weaponBobLocalPos.y = Mathf.Abs(vBobValue);          

                // �÷��̾��� ���� �������� ������ ��ġ�� ����
                lastCharPos = playerCharacterController.transform.position;
            }
        }
    }
}