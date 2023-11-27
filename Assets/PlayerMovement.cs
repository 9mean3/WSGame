using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Windows;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private InputReader inputReader;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float jumpPower = 3f;
    [SerializeField] private float desiredRotationSpeed = 0.3f;
    [SerializeField] private float allowPlayerRotation = 0.1f;

    private CharacterController characterController;

    public bool isGrounded => characterController.isGrounded;

    private bool isSprint;

    private float verticalVelocity;
    private Vector3 movementVelocity;
    private Vector2 inputDir;
    private Vector3 desiredMoveDirection;
    public bool blockRotationPlayer = false; //공격중 플레이어가 회전X.

    [SerializeField] private PlayerAnimation animator;

    [SerializeField] private GameObject CinemachineCameraTarget;
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    [SerializeField] float BottomClamp;
    [SerializeField] float TopClamp;

    [SerializeField] float CameraAngleOverride = 0;

    [SerializeField] private Camera mainCam;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<PlayerAnimation>();
        inputReader.MovementEvent += SetMovement;
        inputReader.JumpEvent += Jump;
        inputReader.SprintEvent += SetSprint;

        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;
    }

    private void OnDestroy()
    {
        inputReader.MovementEvent -= SetMovement;
        inputReader.JumpEvent -= Jump;
    }

    private void SetMovement(Vector2 dir)
    {
        inputDir = dir;
    }

    private void SetSprint(bool value)
    {
        isSprint = value;
    }

    private void CalculatePlayerMovement()
    {
        float tSpeed = isSprint ? moveSpeed : sprintSpeed;

        var forward = mainCam.transform.forward;
        var right = mainCam.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        desiredMoveDirection = forward * inputDir.y + right * inputDir.x;

        if (!blockRotationPlayer && inputDir.sqrMagnitude > allowPlayerRotation)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredMoveDirection), desiredRotationSpeed);
        }
        movementVelocity = desiredMoveDirection * (tSpeed * Time.fixedDeltaTime);
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (inputReader.Look.sqrMagnitude >= 0.01)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = 1.0f;

            _cinemachineTargetYaw += inputReader.Look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += inputReader.Look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    public void RotateToCamera(Transform t)
    {
        var forward = mainCam.transform.forward;

        var rot = transform.rotation;
        desiredMoveDirection = forward;
        Quaternion lookAtRotation = Quaternion.LookRotation(desiredMoveDirection);
        Quaternion lookAtRotationOnlyY = Quaternion.Euler(rot.eulerAngles.x, lookAtRotation.eulerAngles.y, rot.eulerAngles.z);

        t.rotation = Quaternion.Slerp(rot, lookAtRotationOnlyY, desiredRotationSpeed);
    }

    private void ApplyGravity()
    {
        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -1;
        }
        else
        {
            verticalVelocity += gravity * Time.fixedDeltaTime;
        }

        movementVelocity.y = verticalVelocity;
    }

    private void Move()
    {
        characterController.Move(movementVelocity);
    }

    public void Jump()
    {
        if (!isGrounded) return;
        verticalVelocity += jumpPower;
    }

    private void ApplyAnimation()
    {
        float speed = movementVelocity.magnitude;

        animator.SetBlendHash(speed);
    }

    private void FixedUpdate()
    {
        Move();
        CalculatePlayerMovement();
        ApplyGravity();
        ApplyAnimation();
    }
}
