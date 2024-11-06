using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.FPS.Game;
using UnityEngine.UIElements;
using UnityEngine.Events;
using UnityEditor.ShaderGraph.Internal;


// 플레이어가 가진 무기(WeaponCon)들을 관리하는 클래스
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
        // 무기 지금 - 게임을 시작할 때 처음 유저에게 지급되는 무기 리스트
        public List<WeaponCon> startingWeapons = new List<WeaponCon>();

        // 무기 장착

        // 무기를 장착하는 오브젝트 
        public Transform weaponParentSocket;

        // 플레이어가 게임중에 들고 다니는 무기 리스트
        private WeaponCon[] weaponSlots = new WeaponCon[9];

        // 무기 리스트를 관리하는 인덱스
        public int ActiveWeaponIndex {  get; private set; }

        // 무기 교체 - 등록된 함수 호출
        public UnityAction<WeaponCon> OnSwitchToWeapon;

        // 무기 교체 시 상태
        private WeaponSwitchState weaponSwitchState;

        // 무기 교체 시 계산되는 최종 위치
        private Vector3 weaPonMainLocalPos;
        public Transform defaultWeaponPos;
        public Transform downWeaponPos;
        public Transform aimingWeaponPos;
        // 새로 바뀌는 무기 인덱스
        private int weaponSwitchNewIndex;
        private float weaponSwitchTimeStarted = 0f;
        public float weaponSwitchDelay = 1f;

        // 적 포착
        public bool IsPointEnemy { get; private set; }
        public Camera weaponCam;

        // 조준
        // 카메라 셋팅
        // 기본 Fov 값
        public float defaultFov = 60f;
        // Fov 연산계수
        public float weaponFovMult = 1f;
        // 조준 여부
        public bool IsAiming { get; private set; }
        // 무기 이동, Fov 연출 속도
        public float aimingAniSpeed = 10f;

        // 흔들림
        public float bobFrequency = 10f;
        public float bobShrpness = 10f;
        // 기본 흔들림
        public float defaultBobAmount = 0.05f;
        // 조준 흔들림
        public float aimingBobAmount = 0.02f;
        // 흔들림 계수
        private float weaponBobFactor;
        // 현재 프레임에서 이동속도를 계산
        private Vector3 lastCharPos;
        // 흔들림 량 최종 계산값, 이동하지 않으면 0
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

            // 조준 입력값 처리
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

            // 무기 최종 위치
            weaponParentSocket.localPosition = weaPonMainLocalPos + weaponBobLocalPos;
        }

        void Init()
        {
            // 초기화
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
            // 적 포착
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

        // 상태에 따른 무기 연출
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

            // 지연시간 이후 무기 상태 바꾸기
            if (switchingTimeFactor >= 1f)
            {
                if(weaponSwitchState == WeaponSwitchState.PutDownPrvious)
                {
                    // 현재 무기 false, 새로운 무기 true
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

            // 지연시간동안 무기의 위치 이동
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

        // weaponSlots에 무기 프리팹으로 생성한 WeaponCon 오브젝트 추가
        public bool AddWeapon(WeaponCon weaponPrefab)
        {
            // 추가하는 무기 소지 여부 체크 - 중복검사
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

        // 매개변수로 들어온 무기가 기존에 있는지 확인
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
        // 무기 바꾸기, 현재 들고 있는 무기 false, 새로운 무기 true
        public void SwitchWeapon(bool ascendingOrder)
        {
            int newWeaponIndex = -1; // 새로 액티브할 무기 인덱스
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

            // 새로 액티브할 무기 인덱스로 무긱 교체
            SwitchToWeaponIndex(newWeaponIndex);

        }

        void SwitchToWeaponIndex(int newWeaponIndex)
        {
            // newWeaponIndex 값 체크
            if (newWeaponIndex >= 0 && newWeaponIndex != ActiveWeaponIndex)
            {
                weaponSwitchNewIndex = newWeaponIndex;

                weaponSwitchTimeStarted = Time.time;

                // 현재 액티브한 무기가 있는지
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

        // 지정된 슬롯에 무기가 있는지 여부
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

        // 카메라 Fov 값 세팅 : 줌인, 줌아웃
        void SetFov(float fov)
        {
            playerCharacterController.PlayerCamera.fieldOfView = fov;
            weaponCam.fieldOfView = fov * weaponFovMult;
        }

        // 무기 조준에 따른 연출, 무기 위치 조정, Fov 값 조정
        void UpdateWeapomAiming()
        {
            // 무기를 들고 있을때만 조준 가능
            if (weaponSwitchState == WeaponSwitchState.Up)
            {
                WeaponCon activeWeapon = GetActiveWeapon();

                //if (activeWeapon == null)
                //    return;

                // 조준시 : Default -> Aiming 위치로 이동
                if (IsAiming && activeWeapon)
                {
                    // 조준
                    weaPonMainLocalPos = Vector3.Lerp(weaPonMainLocalPos, aimingWeaponPos.localPosition + activeWeapon.aimOffset, aimingAniSpeed * Time.deltaTime);

                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView, activeWeapon.aimZoomRatio * defaultFov, aimingAniSpeed * Time.deltaTime);
                    
                    SetFov(fov);
                }
                else
                {
                    // 조준 해제
                    weaPonMainLocalPos = Vector3.Lerp(weaPonMainLocalPos, defaultWeaponPos.localPosition - activeWeapon.aimOffset, aimingAniSpeed * Time.deltaTime);

                    float fov = Mathf.Lerp(playerCharacterController.PlayerCamera.fieldOfView, defaultFov, aimingAniSpeed * Time.deltaTime);


                    SetFov(fov);
                }
            }   
        }

        // 이동에 의한 무기 흔들림 값
        void UpdateWeaponBob()
        {
            if(Time.deltaTime > 0)
            {
                // 플레이어가 한 프레임동안 이동한 거리
                Vector3 distance = playerCharacterController.transform.position - lastCharPos;
                // 현재 프레임에서 플레이어 이동 속도
                Vector3 playerVelocity = distance / Time.deltaTime;
                float movementFactor = 0f;
                // 흔들림량 (기본, 조준)       
                float bobAmount;
                float frequency = bobFrequency;

                if (playerCharacterController.IsGrounded)
                {
                    movementFactor = Mathf.Clamp01(playerVelocity.magnitude / (playerCharacterController.MaxSpeedOnGround * playerCharacterController.SprintSpeedModifier));
                }

                // 속도에 의한 흔들림 계수
                weaponBobFactor = Mathf.Lerp(weaponBobFactor, movementFactor, bobShrpness * Time.deltaTime);

                if(IsAiming)
                {
                    bobAmount = aimingBobAmount;
                }
                else
                {
                    bobAmount = defaultBobAmount;
                }

                // 좌우 흔들림
                float hBobValue = Mathf.Sin(Time.time * frequency) * bobAmount * weaponBobFactor;
                float vBobValue = ((Mathf.Sin(Time.time * frequency) * 0.5f) + 0.5f) * bobAmount * weaponBobFactor;


                weaponBobLocalPos.x = hBobValue;
                weaponBobLocalPos.y = Mathf.Abs(vBobValue);          

                // 플레이어의 현재 프레임의 마지막 위치를 저장
                lastCharPos = playerCharacterController.transform.position;
            }
        }
    }
}