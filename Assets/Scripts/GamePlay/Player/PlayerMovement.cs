using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D = null;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue input)
    {
        Vector2 moveInput = input.Get<Vector2>();
        _rigidbody2D.linearVelocityX = moveInput.x * 5f;
    }
}
