using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimation : MonoBehaviour
{
    private Animator _animator = null;
    private SpriteRenderer _spriteRenderer = null;
    private Rigidbody2D _rigidbody2D = null;

    private void Awake()
    {
        _animator = GetComponentInChildren<Animator>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    private void OnMove(InputValue input)
    {
        Vector2 moveInput = input.Get<Vector2>();
        if(moveInput.x > 0.1f || moveInput.x < -0.1f)
        {
            _animator.SetBool("IsRunning", true);
            if(moveInput.x > 0)
            {
                _spriteRenderer.flipX = false;
            }
            else if(moveInput.x < 0)
            {
                _spriteRenderer.flipX = true;
            }
        }
        else
        {
            _animator.SetBool("IsRunning", false);
        }
    }

    private void OnJump(InputValue input)
    {
        if(input.isPressed)
        {
            _animator.SetBool("IsJumping", true);
            _animator.SetBool("IsFalling", true);
        }
    }

    private void OnInteract(InputValue input)
    {

    }

    private void Update()
    {
        if(_rigidbody2D.linearVelocityY < -0.1f)
        {
            _animator.SetBool("IsFalling", true);
            _animator.SetBool("IsJumping", false);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            _animator.SetBool("IsJumping", false);
            _animator.SetBool("IsFalling", false);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Ground"))
        {
            _animator.SetBool("IsFalling", true);
        }
    }
}
