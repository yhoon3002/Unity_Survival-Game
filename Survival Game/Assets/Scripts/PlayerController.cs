using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 스피드 조정 변수
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    private float applySpeed;
    [SerializeField]
    private float crouchSpeed;

    // 점프 
    [SerializeField]
    private float jumpForce;

    // 상태 변수 (걷는건지 뛰는건지 구분)
    // isGround를 설정해주는 이유 : 땅에서만 점프가 되게 하기 위해 / 안해주면 공중에서 또 한번 점프가 될 수 있기 때문
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    // 앉았을 때 얼마나 앉을지 결정하는 변수
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // 땅 착지 여부
    private CapsuleCollider capsuleCollider;

    // 카메라의 민감도
    [SerializeField]
    private float lookSensitivity;

    // 카메라의 각도의 한계
    [SerializeField]
    private float cameraRotationLimit;
    // 0은 생략해도 됨 -> 기본값이 0임
    private float currentCameraRotationX = 0;

    // 필요한 컴포넌트
    [SerializeField]
    private Camera theCamera;
    // Rigidbody : 캐릭터의 실제 육체적인 몸
    private Rigidbody myRigid;

    // Start is called before the first frame update
    // 스크립트가 처음 실행되고 실행되는 것
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        // FindObjectOfType : 모든 객체를 조사하여 Camera라는 것을 찾게 됨. 하지만, Camera가 여러 개 일수도 있으므로, 이 방법은 사용하지 않음.
        // theCamera = FindObjectOfType<Camera>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;

        // 초기화
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    // 1초에 보통 60회 정도 업데이트됨
    void Update()
    {
        IsGround();
        TryJump();
        // 뛰는지 걷는지 움직이기 전에 판단해야 하므로 Move() 함수 위에 있어야 함
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }

    // 앉기 시도
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // 앉기 동작
    private void Crouch()
    {   
        // isCrouch가 true이면 false / false이면 true로 바꿔줌
        isCrouch = !isCrouch;

        if (isCrouch)
        {
            applySpeed = crouchSpeed;
            applyCrouchPosY = crouchPosY;
        } else
        {
            applySpeed = walkSpeed;
            applyCrouchPosY = originPosY;
        }

        // x, y, z를 각각 한개씩 수정할 수 없어서 Vector3를 사용하여 바꿔주어야 한다.
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);

        StartCoroutine(CrouchCoroutine());
    }

    // 병렬 처리 됨
    // 부드러운 앉기 동작 실행
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY)
        {
            count++;
            // 보간을 이용
            _posY = Mathf.Lerp(_posY, applyCrouchPosY, 0.3f);
            theCamera.transform.localPosition = new Vector3(0, _posY, 0);

            if(count > 15)
            {
                break;
            }
            yield return null;
        }
        theCamera.transform.localPosition = new Vector3(0, applyCrouchPosY, 0f);

    }

    // 지면 체크
    private void IsGround()
    {
        // Raycast(레이저를 쏠 위치, 어느 방향으로, 어느 거리만큼
        // capsuleCollider.bounds.extents.y : capsuleCollider의 y값의 절반
        // + 0.1f를 해주는 이유 : 계단이나 경사면에서는 땅에 닿아있는 것처럼 보이지만 약간의 오차가 발생해서 그 오차를 상쇄시키기 위해 +0.1f를 해줌
        // 땅에 닿아있다면 true, 땅에 닿아있지 않다면 false를 반환함
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    // 점프 시도
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // 점프
    private void Jump()
    {
        // 앉은 상태에서 점프시 앉은 상태 해제
        if (isCrouch)
        {
            Crouch();
        }
        myRigid.velocity = transform.up * jumpForce;
    }

    // 달리기 시도
    private void TryRun()
    {
        // GetKey : Key가 계속 눌러져 있는 상태
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    // 달리기 실행
    private void Running()
    {
        if (isCrouch)
        {
            Crouch();
        }
        isRun = true;
        applySpeed = runSpeed;
    }

    // 달리기 취소
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // 움직임 실행
    private void Move()
    {
        // Horizontal은 unity에서 기본적으로 Horizontal로 설정해두었음.
        // 왼쪽 -1, 오른쪽 1
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        // transform : 기본 컴포넌트가 가지고 있는 방향값, 위치값
        // 기본값으로 (1, 0, 0)이 들어가게 되어있음
        // 이 때 _moveDirX를 곱해주게 되는데 오른쪽방향키를 누르게 되면 (1, 0, 0) * 1 => 오른쪽으로 이동
        // 왼쪽 방향키를 누르게 되면 (1, 0, 0) * -1 => 왼쪽으로 이동
        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        // (1, 0, 0) + (0, 0, 1)
        // (1, 0, 1) = 2
        // (0.5, 0, 0.5) = 1
        // 유니티에서 내부 알고리즘 상으로 1이 나오면 속도가 더 빠르고, 계산하기 편하게 합을 1로 만들어주는 것임
        // applySpeed에 따라 걷는 속도와 뛰는 속도가 구분됨
        // applySpeed가 없다면 걷는 함수와 뛰는 함수 두개를 나눠줘야 함
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        // Time.deltaTime : Update 내장 함수가 매 프레임마다 실행되기 때문에
        // 아래와 같이 만들어주어서 실행되게 함
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    // 좌우 캐릭터 회전
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        // myRigid.rotation : 자기 자신의 회전값
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

        // Debug.Log(myRigid.rotation);
        // Debug.Log(myRigid.rotation.eulerAngles);
    }

    private void CameraRotation()
    {
        // 상하 카메라 회전

        // 마우스는 3차원이 아님 x와 y밖에 없음. Mouse Y는 마우스를 위아래로 이동시킬때
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        // 마우스를 위로 올렸을 때 고개를 위로 확 올리면 안되기때문에 lookSensitivity를 주었음 -> 어느정도 천천히 움직이게
        float _cameraRotationX = _xRotation * lookSensitivity;

        currentCameraRotationX -= _cameraRotationX;
        // Mathf.Clamp : 가두는 함수
        // 따라서, 아래는 -45도 ~ 45도 까지의 각도로 가두는 것임.
        // 60이 들어오게 되면, 최댓값인 45로 고정이 됨.
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // localEulerAngles : Unity의 Rotation X, Y, Z를 생각하면 됨
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}
