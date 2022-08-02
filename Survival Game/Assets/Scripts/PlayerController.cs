using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed;

    // ī�޶��� �ΰ���
    [SerializeField]
    private float lookSensitivity;

    // ī�޶��� ����
    [SerializeField]
    private float cameraRotationLimit;
    // 0�� �����ص� �� -> �⺻���� 0��
    private float currentCameraRotationX = 0;

    [SerializeField]
    private Camera theCamera;
    // Rigidbody : ĳ������ ���� ��ü���� ��
    private Rigidbody myRigid;

    // Start is called before the first frame update
    // ��ũ��Ʈ�� ó�� ����ǰ� ����Ǵ� ��
    void Start()
    {
        // FindObjectOfType : ��� ��ü�� �����Ͽ� Camera��� ���� ã�� ��. ������, Camera�� ���� �� �ϼ��� �����Ƿ�, �� ����� ������� ����.
        // theCamera = FindObjectOfType<Camera>();
        myRigid = GetComponent<Rigidbody>();   
    }

    // Update is called once per frame
    // 1�ʿ� ���� 60ȸ ���� ������Ʈ��
    void Update()
    {
        Move();
        CameraRotation();
        CharacterRotation();
    }

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
        Vector3 _velocity = (_moveHorizontal + _moveVertical).normalized * walkSpeed;

        // Time.deltaTime : Update ���� �Լ��� �� �����Ӹ��� ����Ǳ� ������
        // �Ʒ��� ���� ������־ ����ǰ� ��
        myRigid.MovePosition(transform.position + _velocity * Time.deltaTime);

    }

    private void CharacterRotation()
    {
        // �¿� ĳ���� ȸ��
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
