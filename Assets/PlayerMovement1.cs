using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement1 : MonoBehaviour
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

    private float _verticalVelocity; //세로 이동속도
    private Vector3 _movementVelocity;
    private Vector2 _inputDir;
    private Vector3 _desiredMoveDirection;
    public bool blockRotationPlayer = false; //사격중일때는 플레이어가 회전하지 않도록 한다. 

    //[SerializeField] private PlayerAnimator _animator;


    private Camera _mainCam;
    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _reader.MovementEvent += SetMovement;
        _reader.JumpEvent += Jump;
        _mainCam = Camera.main; //메인카메라 캐싱
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
        { //발사중이 아니라면 천천히 가는방향으로 회전
            _modelTrm.rotation = Quaternion.Slerp(_modelTrm.rotation, Quaternion.LookRotation(_desiredMoveDirection), _desiredRotationSpeed);
        }
        _movementVelocity = _desiredMoveDirection * (_moveSpeed * Time.fixedDeltaTime);
    }

    //사격시작시 카메라방향으로 돌리기 위함.
    public void RotateToCamera(Transform t)
    {
        var forward = _mainCam.transform.forward;

        //y축 회전만 가져와서 SLerp시킴
        var rot = _modelTrm.rotation;
        _desiredMoveDirection = forward;
        Quaternion lookAtRotation = Quaternion.LookRotation(_desiredMoveDirection);
        Quaternion lookAtRotationOnlyY = Quaternion.Euler(rot.eulerAngles.x, lookAtRotation.eulerAngles.y, rot.eulerAngles.z);

        t.rotation = Quaternion.Slerp(rot, lookAtRotationOnlyY, _desiredRotationSpeed);
    }

    private void ApplyGravity()
    {
        if (IsGrounded && _verticalVelocity < 0)  //땅에 착지 상태
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
        //_animator.SetShooting(blockRotationPlayer); //블록킹상태면 슈팅으로 변경

        float speed = _inputDir.sqrMagnitude;

        //_animator.SetBlendValue(speed);
        //_animator.SetXY(_inputDir);
    }

    private void FixedUpdate()
    {
        ApplyAnimation();
        CalculatePlayerMovement();
        ApplyGravity(); //중력 적용
        Move();
    }


}
