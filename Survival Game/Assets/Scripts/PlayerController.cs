using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;

    // 카메라의 민감도
    [SerializeField]
    private float lookSensitivity;

    // 카메라의 각도
    [SerializeField]
    private float cameraRotationLimit;
    // 0은 생략해도 됨 -> 기본값이 0임
    private float currentCameraRotationX = 0;

    [SerializeField]
    private Camera theCamera;
    // Rigidbody : 캐릭터의 실제 육체적인 몸
    private Rigidbody myRigid;

    // Start is called before the first frame update
    // 스크립트가 처음 실행되고 실행되는 것
    void Start()
    {
        // FindObjectOfType : 모든 객체를 조사하여 Camera라는 것을 찾게 됨. 하지만, Camera가 여러 개 일수도 있으므로, 이 방법은 사용하지 않음.
        // theCamera = FindObjectOfType<Camera>();
        myRigid = GetComponent<Rigidbody>();   
    }

    // Update is called once per frame
    // 1초에 보통 60회 정도 업데이트됨
    void Update()
    {
        Move();
        CameraRotation();
        CharacterRotation();
    }

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
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * walkSpeed;

        // Time.deltaTime : Update 내장 함수가 매 프레임마다 실행되기 때문에
        // 아래와 같이 만들어주어서 실행되게 함
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    private void CharacterRotation()
    {
        // 좌우 캐릭터 회전
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
