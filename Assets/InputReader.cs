using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(fileName = "New Input Reader", menuName = "SO/Input/InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event Action<bool> FireEvent;
    public event Action JumpEvent;
    public event Action<Vector2> MovementEvent;
    public Vector2 AimDelta { get; private set; } //���콺�� �̺�Ʈ����� �ƴϱ� ������

    private Controls _playerInputAction; //�̱������� ����� �༮

    private void OnEnable()
    {
        if (_playerInputAction == null)
        {
            _playerInputAction = new Controls();
            _playerInputAction.Player.SetCallbacks(this); //�÷��̾� ��ǲ�� �߻��ϸ� �� �ν��Ͻ��� �������ְ�
        }

        _playerInputAction.Player.Enable(); //Ȱ��ȭ
    }

    public void OnMovement(InputAction.CallbackContext context)
    {
        Vector2 value = context.ReadValue<Vector2>();
        MovementEvent?.Invoke(value);
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            FireEvent?.Invoke(true);
        }
        else if (context.canceled)
        {
            FireEvent?.Invoke(false);
        }

    }

    public void OnAim(InputAction.CallbackContext context)
    {
        AimDelta = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            JumpEvent?.Invoke();
    }
}