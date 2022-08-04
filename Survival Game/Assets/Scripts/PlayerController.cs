using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���ǵ� ���� ����
    [SerializeField]
    private float walkSpeed;
    [SerializeField]
    private float runSpeed;
    private float applySpeed;
    [SerializeField]
    private float crouchSpeed;

    // ���� 
    [SerializeField]
    private float jumpForce;

    // ���� ���� (�ȴ°��� �ٴ°��� ����)
    // isGround�� �������ִ� ���� : �������� ������ �ǰ� �ϱ� ���� / �����ָ� ���߿��� �� �ѹ� ������ �� �� �ֱ� ����
    private bool isRun = false;
    private bool isCrouch = false;
    private bool isGround = true;

    // �ɾ��� �� �󸶳� ������ �����ϴ� ����
    [SerializeField]
    private float crouchPosY;
    private float originPosY;
    private float applyCrouchPosY;

    // �� ���� ����
    private CapsuleCollider capsuleCollider;

    // ī�޶��� �ΰ���
    [SerializeField]
    private float lookSensitivity;

    // ī�޶��� ������ �Ѱ�
    [SerializeField]
    private float cameraRotationLimit;
    // 0�� �����ص� �� -> �⺻���� 0��
    private float currentCameraRotationX = 0;

    // �ʿ��� ������Ʈ
    [SerializeField]
    private Camera theCamera;
    // Rigidbody : ĳ������ ���� ��ü���� ��
    private Rigidbody myRigid;

    // Start is called before the first frame update
    // ��ũ��Ʈ�� ó�� ����ǰ� ����Ǵ� ��
    void Start()
    {
        capsuleCollider = GetComponent<CapsuleCollider>();
        // FindObjectOfType : ��� ��ü�� �����Ͽ� Camera��� ���� ã�� ��. ������, Camera�� ���� �� �ϼ��� �����Ƿ�, �� ����� ������� ����.
        // theCamera = FindObjectOfType<Camera>();
        myRigid = GetComponent<Rigidbody>();
        applySpeed = walkSpeed;

        // �ʱ�ȭ
        originPosY = theCamera.transform.localPosition.y;
        applyCrouchPosY = originPosY;
    }

    // Update is called once per frame
    // 1�ʿ� ���� 60ȸ ���� ������Ʈ��
    void Update()
    {
        IsGround();
        TryJump();
        // �ٴ��� �ȴ��� �����̱� ���� �Ǵ��ؾ� �ϹǷ� Move() �Լ� ���� �־�� ��
        TryRun();
        TryCrouch();
        Move();
        CameraRotation();
        CharacterRotation();
    }

    // �ɱ� �õ�
    private void TryCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }
    }

    // �ɱ� ����
    private void Crouch()
    {   
        // isCrouch�� true�̸� false / false�̸� true�� �ٲ���
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

        // x, y, z�� ���� �Ѱ��� ������ �� ��� Vector3�� ����Ͽ� �ٲ��־�� �Ѵ�.
        //theCamera.transform.localPosition = new Vector3(theCamera.transform.localPosition.x, applyCrouchPosY, theCamera.transform.localPosition.z);

        StartCoroutine(CrouchCoroutine());
    }

    // ���� ó�� ��
    // �ε巯�� �ɱ� ���� ����
    IEnumerator CrouchCoroutine()
    {
        float _posY = theCamera.transform.localPosition.y;
        int count = 0;

        while(_posY != applyCrouchPosY)
        {
            count++;
            // ������ �̿�
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

    // ���� üũ
    private void IsGround()
    {
        // Raycast(�������� �� ��ġ, ��� ��������, ��� �Ÿ���ŭ
        // capsuleCollider.bounds.extents.y : capsuleCollider�� y���� ����
        // + 0.1f�� ���ִ� ���� : ����̳� ���鿡���� ���� ����ִ� ��ó�� �������� �ణ�� ������ �߻��ؼ� �� ������ ����Ű�� ���� +0.1f�� ����
        // ���� ����ִٸ� true, ���� ������� �ʴٸ� false�� ��ȯ��
        isGround = Physics.Raycast(transform.position, Vector3.down, capsuleCollider.bounds.extents.y + 0.1f);
    }

    // ���� �õ�
    private void TryJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            Jump();
        }
    }

    // ����
    private void Jump()
    {
        // ���� ���¿��� ������ ���� ���� ����
        if (isCrouch)
        {
            Crouch();
        }
        myRigid.velocity = transform.up * jumpForce;
    }

    // �޸��� �õ�
    private void TryRun()
    {
        // GetKey : Key�� ��� ������ �ִ� ����
        if (Input.GetKey(KeyCode.LeftShift))
        {
            Running();
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            RunningCancel();
        }
    }

    // �޸��� ����
    private void Running()
    {
        if (isCrouch)
        {
            Crouch();
        }
        isRun = true;
        applySpeed = runSpeed;
    }

    // �޸��� ���
    private void RunningCancel()
    {
        isRun = false;
        applySpeed = walkSpeed;
    }

    // ������ ����
    private void Move()
    {
        // Horizontal�� unity���� �⺻������ Horizontal�� �����صξ���.
        // ���� -1, ������ 1
        float _moveDirX = Input.GetAxisRaw("Horizontal");
        float _moveDirZ = Input.GetAxisRaw("Vertical");

        // transform : �⺻ ������Ʈ�� ������ �ִ� ���Ⱚ, ��ġ��
        // �⺻������ (1, 0, 0)�� ���� �Ǿ�����
        // �� �� _moveDirX�� �����ְ� �Ǵµ� �����ʹ���Ű�� ������ �Ǹ� (1, 0, 0) * 1 => ���������� �̵�
        // ���� ����Ű�� ������ �Ǹ� (1, 0, 0) * -1 => �������� �̵�
        Vector3 _moveHorizontal = transform.right * _moveDirX;
        Vector3 _moveVertical = transform.forward * _moveDirZ;

        // (1, 0, 0) + (0, 0, 1)
        // (1, 0, 1) = 2
        // (0.5, 0, 0.5) = 1
        // ����Ƽ���� ���� �˰��� ������ 1�� ������ �ӵ��� �� ������, ����ϱ� ���ϰ� ���� 1�� ������ִ� ����
        // applySpeed�� ���� �ȴ� �ӵ��� �ٴ� �ӵ��� ���е�
        // applySpeed�� ���ٸ� �ȴ� �Լ��� �ٴ� �Լ� �ΰ��� ������� ��
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * applySpeed;

        // Time.deltaTime : Update ���� �Լ��� �� �����Ӹ��� ����Ǳ� ������
        // �Ʒ��� ���� ������־ ����ǰ� ��
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    // �¿� ĳ���� ȸ��
    private void CharacterRotation()
    {
        float _yRotation = Input.GetAxisRaw("Mouse X");
        Vector3 _characterRotationY = new Vector3(0f, _yRotation, 0f) * lookSensitivity;

        // myRigid.rotation : �ڱ� �ڽ��� ȸ����
        myRigid.MoveRotation(myRigid.rotation * Quaternion.Euler(_characterRotationY));

        // Debug.Log(myRigid.rotation);
        // Debug.Log(myRigid.rotation.eulerAngles);
    }

    private void CameraRotation()
    {
        // ���� ī�޶� ȸ��

        // ���콺�� 3������ �ƴ� x�� y�ۿ� ����. Mouse Y�� ���콺�� ���Ʒ��� �̵���ų��
        float _xRotation = Input.GetAxisRaw("Mouse Y");
        // ���콺�� ���� �÷��� �� ���� ���� Ȯ �ø��� �ȵǱ⶧���� lookSensitivity�� �־��� -> ������� õõ�� �����̰�
        float _cameraRotationX = _xRotation * lookSensitivity;

        currentCameraRotationX -= _cameraRotationX;
        // Mathf.Clamp : ���δ� �Լ�
        // ����, �Ʒ��� -45�� ~ 45�� ������ ������ ���δ� ����.
        // 60�� ������ �Ǹ�, �ִ��� 45�� ������ ��.
        currentCameraRotationX = Mathf.Clamp(currentCameraRotationX, -cameraRotationLimit, cameraRotationLimit);

        // localEulerAngles : Unity�� Rotation X, Y, Z�� �����ϸ� ��
        theCamera.transform.localEulerAngles = new Vector3(currentCameraRotationX, 0f, 0f);
    }
}
