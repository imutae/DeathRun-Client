using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private void OnMove(InputValue input)
    {
        Vector2 movement = input.Get<Vector2>();
        Debug.Log($"Move: {movement}");
    }

    private void OnJump(InputValue input)
    {
        bool isJumping = input.isPressed;
        Debug.Log($"Jump: {isJumping}");
    }

    private void OnInteract(InputValue input)
    {
        bool isInteracting = input.isPressed;
        Debug.Log($"Interact: {isInteracting}");
    }
}
