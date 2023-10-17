using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputReader _reader;

    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _gravity = -9.8f;
    [SerializeField] private float _jumpPower = 3f;
    [SerializeField] private float _desiredRotationSpeed = 0.3f;
    [SerializeField] private float _allowPlayerRotation = 0.1f;
    [FormerlySerializedAs("_visualTrm")][SerializeField] private Transform _modelTrm;

    private CharacterController _characterController;
    public bool IsGrounded => _characterController.isGrounded;

    private float _verticalVelocity; //���� �̵��ӵ�
    private Vector3 _movementVelocity;
    private Vector2 _inputDir;
    private Vector3 _desiredMoveDirection;
    public bool blockRotationPlayer = false; //������϶��� �÷��̾ ȸ������ �ʵ��� �Ѵ�. 

    //[SerializeField] private PlayerAnimator _animator;


    private Camera _mainCam;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _reader.MovementEvent += SetMovement;
        _reader.JumpEvent += Jump;
        _mainCam = Camera.main; //����ī�޶� ĳ��
        //_animator = _modelTrm.GetComponent<PlayerAnimator>();
    }

    private void OnDestroy()
    {
        _reader.MovementEvent -= SetMovement;
        _reader.JumpEvent -= Jump;
    }

    private void SetMovement(Vector2 value)
    {
        _inputDir = value;
    }

    private void CalculatePlayerMovement()
    {
        var forward = _mainCam.transform.forward;
        var right = _mainCam.transform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();
        _desiredMoveDirection = forward * _inputDir.y + right * _inputDir.x;

        if (blockRotationPlayer == false && _inputDir.sqrMagnitude > _allowPlayerRotation)
        { //�߻����� �ƴ϶�� õõ�� ���¹������� ȸ��
            _modelTrm.rotation = Quaternion.Slerp(_modelTrm.rotation, Quaternion.LookRotation(_desiredMoveDirection), _desiredRotationSpeed);
        }
        _movementVelocity = _desiredMoveDirection * (_moveSpeed * Time.fixedDeltaTime);
    }

    //��ݽ��۽� ī�޶�������� ������ ����.
    public void RotateToCamera(Transform t)
    {
        var forward = _mainCam.transform.forward;

        //y�� ȸ���� �����ͼ� SLerp��Ŵ
        var rot = _modelTrm.rotation;
        _desiredMoveDirection = forward;
        Quaternion lookAtRotation = Quaternion.LookRotation(_desiredMoveDirection);
        Quaternion lookAtRotationOnlyY = Quaternion.Euler(rot.eulerAngles.x, lookAtRotation.eulerAngles.y, rot.eulerAngles.z);

        t.rotation = Quaternion.Slerp(rot, lookAtRotationOnlyY, _desiredRotationSpeed);
    }

    private void ApplyGravity()
    {
        if (IsGrounded && _verticalVelocity < 0)  //���� ���� ����
        {
            _verticalVelocity = -1f;
        }
        else
        {
            _verticalVelocity += _gravity * Time.fixedDeltaTime;
        }

        _movementVelocity.y = _verticalVelocity;
    }

    private void Move()
    {
        _characterController.Move(_movementVelocity);
    }

    public void Jump()
    {
        if (!IsGrounded) return;
        _verticalVelocity += _jumpPower;
    }

    public void ApplyAnimation()
    {
        //_animator.SetShooting(blockRotationPlayer); //���ŷ���¸� �������� ����

        float speed = _inputDir.sqrMagnitude;

        //_animator.SetBlendValue(speed);
        //_animator.SetXY(_inputDir);
    }

    private void FixedUpdate()
    {
        ApplyAnimation();
        CalculatePlayerMovement();
        ApplyGravity(); //�߷� ����
        Move();
    }


}
