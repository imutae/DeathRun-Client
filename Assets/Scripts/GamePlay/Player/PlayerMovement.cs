using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rigidbody2D = null;
    private bool _isWall = false;

    private void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue input)
    {
        if(_isWall)
        {
            return;
        }

        Vector2 moveInput = input.Get<Vector2>();
        _rigidbody2D.linearVelocityX = moveInput.x * 5f;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            ContactPoint2D[] contactPoints = collision.contacts;

            foreach(ContactPoint2D contact in contactPoints)
            {
                if(contact.normal.y < 0.5f && (contact.normal.x > 0.5f || contact.normal.x < -0.5f))
                {
                    _isWall = true;
                    _rigidbody2D.AddForceY(-1f, ForceMode2D.Impulse);
                    return;
                }
            }

            _isWall = false;
        }
    }
}
