using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJump : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D = null;
    private bool _isGround = false;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void OnJump(InputValue input)
    {
        if(input.isPressed)
        {
            Jump();
        }
    }

    private void Jump()
    {
        if(_isGround)
        {
            _rigidbody2D.AddForce(Vector2.up * 5f, ForceMode2D.Impulse);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _isGround = collision.gameObject.CompareTag("Ground");
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            _isGround = false;
        }
    }
}
