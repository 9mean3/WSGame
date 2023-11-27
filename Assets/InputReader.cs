using System;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using static Controls;

[CreateAssetMenu(menuName = "SO/Input")]
public class InputReader : ScriptableObject, IPlayerInputActions
{
    private Controls inputReader;

    public event Action<Vector2> MovementEvent;
    public Vector2 Look;
    public event Action JumpEvent;
    public event Action<bool> SprintEvent;
    public event Action Attack;

    private void OnEnable()
    {
        if (inputReader == null)
        {
            inputReader = new Controls();
            inputReader.PlayerInput.SetCallbacks(this); // �� SOŬ������ ��ǲ�� �� ����
        }
        inputReader.PlayerInput.Enable(); //Ȱ��ȭ
    }


    public void OnMovement(InputAction.CallbackContext context)
    {
        MovementEvent.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look = context.ReadValue<Vector2>();
        Debug.Log(Look);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed)
            JumpEvent.Invoke();
    }

    public void OnSprint(InputAction.CallbackContext context)
    {
        SprintEvent.Invoke(context.performed);
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
            Attack?.Invoke();
    }
}